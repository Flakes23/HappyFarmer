using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HappyFarmer.Shared.Contracts.Auth;

/// <summary>
/// Fetch JWKS từ Auth Service và cache cục bộ (không gọi Auth Service ở mỗi request —
/// chỉ refresh định kỳ). Dùng làm IssuerSigningKeyResolver cho JwtBearer ở các service khác.
/// </summary>
public class RemoteJwksKeyResolver(IHttpClientFactory httpClientFactory, IOptions<RemoteJwtAuthOptions> options, ILogger<RemoteJwksKeyResolver> logger)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private readonly SemaphoreSlim _lock = new(1, 1);
    private List<SecurityKey> _cachedKeys = [];
    private DateTime _cachedAtUtc = DateTime.MinValue;

    public IEnumerable<SecurityKey> ResolveSigningKeys(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters)
    {
        if (DateTime.UtcNow - _cachedAtUtc < CacheDuration && _cachedKeys.Count > 0)
        {
            return _cachedKeys;
        }

        _lock.Wait();
        try
        {
            if (DateTime.UtcNow - _cachedAtUtc >= CacheDuration || _cachedKeys.Count == 0)
            {
                _cachedKeys = FetchKeysAsync().GetAwaiter().GetResult();
                _cachedAtUtc = DateTime.UtcNow;
            }
        }
        finally
        {
            _lock.Release();
        }

        return _cachedKeys;
    }

    private async Task<List<SecurityKey>> FetchKeysAsync()
    {
        try
        {
            var client = httpClientFactory.CreateClient(nameof(RemoteJwksKeyResolver));
            using var stream = await client.GetStreamAsync(options.Value.JwksUrl);
            using var doc = await JsonDocument.ParseAsync(stream);

            var keys = new List<SecurityKey>();
            foreach (var jwk in doc.RootElement.GetProperty("keys").EnumerateArray())
            {
                var n = Base64UrlDecode(jwk.GetProperty("n").GetString()!);
                var e = Base64UrlDecode(jwk.GetProperty("e").GetString()!);
                var key = new RsaSecurityKey(new System.Security.Cryptography.RSAParameters { Modulus = n, Exponent = e })
                {
                    KeyId = jwk.GetProperty("kid").GetString(),
                };
                keys.Add(key);
            }

            return keys;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không thể fetch JWKS từ {JwksUrl}", options.Value.JwksUrl);
            return _cachedKeys; // giữ nguyên cache cũ (nếu có) thay vì làm sập toàn bộ request
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        padded += new string('=', (4 - padded.Length % 4) % 4);
        return Convert.FromBase64String(padded);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using HappyFarmer.AuthService.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HappyFarmer.AuthService.Api.Services;

public class JwtTokenService(RsaKeyProvider keyProvider, IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public int AccessTokenMinutes => _options.AccessTokenMinutes;

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("role", user.Role.ToString()),
            new("fullName", user.FullName),
        };

        if (!string.IsNullOrEmpty(user.PhoneNumber)) claims.Add(new Claim("phone", user.PhoneNumber));
        if (!string.IsNullOrEmpty(user.Email)) claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

        var key = new RsaSecurityKey(keyProvider.Rsa) { KeyId = keyProvider.Kid };
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string PlainText, string Hash, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var plainText = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        var hash = HashRefreshToken(plainText);
        var expiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays);
        return (plainText, hash, expiresAt);
    }

    public static string HashRefreshToken(string plainText) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plainText)));
}
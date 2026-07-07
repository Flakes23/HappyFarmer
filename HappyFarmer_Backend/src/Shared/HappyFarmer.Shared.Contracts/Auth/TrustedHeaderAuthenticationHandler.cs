using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyFarmer.Shared.Contracts.Auth;

public class TrustedHeaderAuthenticationOptions : AuthenticationSchemeOptions;

/// <summary>
/// Xác thực bằng header danh tính (X-User-Id/X-User-Role/X-User-Phone) do API Gateway gắn sau khi
/// tự verify JWT (xem docs/architecture/02-security-auth.md) — service dùng handler này KHÔNG tự
/// verify chữ ký JWT nữa. Chỉ an toàn khi service không bị lộ ra ngoài mạng nội bộ (chỉ Gateway
/// gọi tới được); ở local dev, service vẫn bind port ra host nên request có thể bỏ qua Gateway và
/// tự gắn header giả — chấp nhận trade-off này ở môi trường dev (xem CLAUDE.md).
/// </summary>
public class TrustedHeaderAuthenticationHandler(
    IOptionsMonitor<TrustedHeaderAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<TrustedHeaderAuthenticationOptions>(options, logger, encoder)
{
    public const string SchemeName = "TrustedHeader";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers["X-User-Id"].ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> { new("sub", userId) };

        var role = Request.Headers["X-User-Role"].ToString();
        if (!string.IsNullOrEmpty(role)) claims.Add(new Claim("role", role));

        var phone = Request.Headers["X-User-Phone"].ToString();
        if (!string.IsNullOrEmpty(phone)) claims.Add(new Claim("phone", phone));

        var identity = new ClaimsIdentity(claims, SchemeName, nameType: "sub", roleType: "role");
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

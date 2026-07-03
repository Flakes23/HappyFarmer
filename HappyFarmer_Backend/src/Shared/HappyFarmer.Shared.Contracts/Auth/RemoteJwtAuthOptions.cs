namespace HappyFarmer.Shared.Contracts.Auth;

/// <summary>
/// Cấu hình để 1 service verify JWT do Auth Service phát hành, bằng cách fetch JWKS
/// (xem docs/architecture/02-security-auth.md). Bind từ section "Jwt" trong appsettings.
/// </summary>
public class RemoteJwtAuthOptions
{
    public const string SectionName = "Jwt";

    public required string JwksUrl { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}

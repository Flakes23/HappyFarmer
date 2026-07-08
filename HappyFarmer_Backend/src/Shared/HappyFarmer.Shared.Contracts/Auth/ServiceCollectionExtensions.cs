using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HappyFarmer.Shared.Contracts.Auth;

public static class ServiceCollectionExtensions
{
    // Trình duyệt không thể gắn header Authorization vào WebSocket handshake, nên SignalR JS
    // client gửi token qua query string (?access_token=...) thay vì header. Chỉ chấp nhận
    // fallback này cho đúng path của hub — mọi route khác vẫn bắt buộc header Authorization
    // như cũ. Dùng mảng để hub mới ở service khác sau này chỉ cần thêm prefix vào đây.
    private static readonly string[] HubPathPrefixes = ["/api/marketplace/hubs"];


    /// <summary>
    /// Đăng ký JWT Bearer authentication verify token do Auth Service phát hành,
    /// bằng cách fetch + cache JWKS (xem docs/architecture/02-security-auth.md).
    /// Dùng cho mọi service KHÔNG PHẢI Auth Service.
    /// </summary>
    public static IServiceCollection AddRemoteJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient(nameof(RemoteJwksKeyResolver));
        services.Configure<RemoteJwtAuthOptions>(configuration.GetSection(RemoteJwtAuthOptions.SectionName));
        services.AddSingleton<RemoteJwksKeyResolver>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<RemoteJwksKeyResolver, Microsoft.Extensions.Options.IOptions<RemoteJwtAuthOptions>>((jwtOptions, resolver, remoteOptions) =>
            {
                // Tắt remap claim type ngắn ("role","sub") sang URI dài (ClaimTypes.*) —
                // nếu không, RoleClaimType/NameClaimType bên dưới sẽ không khớp claim thực tế trong token.
                jwtOptions.MapInboundClaims = false;
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = remoteOptions.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = remoteOptions.Value.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeyResolver = resolver.ResolveSigningKeys,
                    NameClaimType = "sub",
                    RoleClaimType = "role",
                };

                jwtOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            HubPathPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Đăng ký xác thực bằng header danh tính (X-User-Id/X-User-Role/X-User-Phone) do API Gateway
    /// gắn — dùng cho service phía sau Gateway, KHÔNG tự verify JWT nữa (xem
    /// TrustedHeaderAuthenticationHandler và docs/architecture/02-security-auth.md).
    /// </summary>
    public static IServiceCollection AddTrustedHeaderAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(TrustedHeaderAuthenticationHandler.SchemeName)
            .AddScheme<TrustedHeaderAuthenticationOptions, TrustedHeaderAuthenticationHandler>(
                TrustedHeaderAuthenticationHandler.SchemeName, _ => { });

        services.AddAuthorization();

        return services;
    }
}

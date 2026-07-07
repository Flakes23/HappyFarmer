using HappyFarmer.Shared.Contracts.Auth;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilderContext =>
    {
        transformBuilderContext.AddRequestTransform(transformContext =>
        {
            // Luôn xoá header danh tính trước, tránh client bên ngoài tự gắn header giả mạo
            // rồi được service phía sau tin nhầm là do Gateway xác thực gắn vào.
            transformContext.ProxyRequest.Headers.Remove("X-User-Id");
            transformContext.ProxyRequest.Headers.Remove("X-User-Role");
            transformContext.ProxyRequest.Headers.Remove("X-User-Phone");

            var user = transformContext.HttpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst("sub")?.Value;
                var role = user.FindFirst("role")?.Value;
                var phone = user.FindFirst("phone")?.Value;

                if (userId is not null) transformContext.ProxyRequest.Headers.Add("X-User-Id", userId);
                if (role is not null) transformContext.ProxyRequest.Headers.Add("X-User-Role", role);
                if (phone is not null) transformContext.ProxyRequest.Headers.Add("X-User-Phone", phone);
            }

            return ValueTask.CompletedTask;
        });
    });

builder.Services.AddRemoteJwtAuthentication(builder.Configuration);

var corsOrigins = builder.Configuration["Cors:AllowedOrigins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();

// Xác thực token nếu có (populate HttpContext.User cho transform ở trên đọc claims),
// nhưng KHÔNG chặn request thiếu token ở đây: trong giai đoạn migration, AuthService và
// MarketPriceService vẫn tự verify + [Authorize] riêng (xem docs/architecture/02-security-auth.md).
// Khi các service đã chuyển sang tin header do Gateway gắn, gắn AuthorizationPolicy theo route
// trong cấu hình ReverseProxy để Gateway tự chặn thay vì để lọt xuống service.
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();

using System.Threading.RateLimiting;
using Google.GenAI;
using HappyFarmer.AiAdvisoryService.Api.Data;
using HappyFarmer.AiAdvisoryService.Api.Services;
using HappyFarmer.Shared.Contracts.Auth;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Qdrant.Client;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình resilience (retry + circuit-breaker + timeout) dùng chung cho 3 HttpClient nội bộ —
// pipeline mặc định của AddStandardResilienceHandler khá "rộng rãi" (tổng timeout ~30s, nhiều lần
// retry), không phù hợp vì các lệnh gọi này nằm TRONG 1 lượt chat của user (đã có MaxToolIterations
// riêng ở GeminiChatService) — thu hẹp lại để 1 tool-call chậm không kéo dài cả lượt chat.
void ConfigureInternalResilience(HttpStandardResilienceOptions options)
{
    // AttemptTimeout phải nhỏ hơn TotalRequestTimeout (validate lúc startup) — mỗi lần thử tối đa 2s,
    // tổng cả retry tối đa 5s, để 1 tool-call chậm không kéo dài cả lượt chat của user.
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
    options.Retry.MaxRetryAttempts = 2;
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
}

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AiAdvisoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AiAdvisoryDb")));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddScoped<ChatContextCacheService>();
builder.Services.AddScoped<DailyQuotaService>();

builder.Services.AddSingleton(_ => new Client(apiKey: builder.Configuration["Gemini:ApiKey"]));
builder.Services.AddScoped<GeminiChatService>();
builder.Services.AddScoped<GeminiHarvestPredictionService>();
builder.Services.AddScoped<GeminiDiseaseDetectionService>();

// OpenWeatherMap: API public bên ngoài, URL cố định (không cần config theo môi trường như
// AuthServiceClient — đó là service nội bộ đổi host giữa local/production).
builder.Services.AddHttpClient("OpenWeatherMap", client =>
{
    client.BaseAddress = new Uri("https://api.openweathermap.org/");
});
builder.Services.AddScoped<OpenWeatherMapClient>();
builder.Services.AddScoped<WeatherCacheService>();

// Tải ảnh disease-detection từ URL Cloudinary (frontend upload thẳng lên Cloudinary trước) —
// không BaseAddress vì URL đến từ Cloudinary, không cố định. Cần User-Agent rõ ràng vì nhiều host
// (Cloudinary/CDN, kể cả ảnh test từ Wikimedia) chặn request thiếu/có User-Agent mặc định của HttpClient.
builder.Services.AddHttpClient("ImageDownload", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("HappyFarmer/1.0 (AI Advisory Service disease detection)");
});

builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection(CloudinaryOptions.SectionName));
builder.Services.AddScoped<CloudinarySignatureService>();

// Gọi service khác qua network nội bộ (không qua Gateway) để chatbot tra giá/tin đăng/thông tin
// người dùng thật qua function calling — cùng pattern AuthServiceClient đã có ở Marketplace Service.
builder.Services.AddHttpClient("MarketPriceService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:MarketPriceServiceBaseUrl"]!);
}).AddStandardResilienceHandler(ConfigureInternalResilience);
builder.Services.AddHttpClient("MarketplaceService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:MarketplaceServiceBaseUrl"]!);
}).AddStandardResilienceHandler(ConfigureInternalResilience);
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AuthServiceBaseUrl"]!);
}).AddStandardResilienceHandler(ConfigureInternalResilience);
builder.Services.AddScoped<MarketPriceServiceClient>();
builder.Services.AddScoped<MarketplaceServiceClient>();
builder.Services.AddScoped<AuthServiceClient>();

// RAG — Qdrant lưu vector + payload (nội dung chunk tài liệu nông nghiệp), Gemini Embedding API
// tạo vector. Xem Controllers/InternalController.cs (nhận ingest từ HappyFarmer.RagIngestor) và
// GeminiChatService.cs (tool search_knowledge_base).
builder.Services.AddSingleton(_ => new QdrantClient(
    builder.Configuration["Qdrant:Host"] ?? "localhost",
    builder.Configuration.GetValue("Qdrant:Port", 6334)));
builder.Services.AddScoped<GeminiEmbeddingService>();
builder.Services.AddScoped<QdrantKnowledgeService>();

builder.Services.AddTrustedHeaderAuthentication();

builder.Services.AddRateLimiter(options =>
{
    // Concurrency limiter có hàng đợi — bảo vệ hệ thống khỏi bắn quá nhiều request đồng thời
    // tới Gemini API. Request vượt PermitLimit sẽ XẾP HÀNG (không bị từ chối ngay) tới khi đủ
    // QueueLimit thì mới trả 503. Đây là kiểm soát tải ở tầng hệ thống, tách biệt với quota/ngày
    // theo user (DailyQuotaService — đó là business rule, không phải bảo vệ hệ thống).
    // Dùng CHUNG 1 policy cho cả chat và harvest-prediction vì cả 2 chia sẻ giới hạn thật của
    // cùng 1 tài khoản Gemini — tách 2 policy sẽ vô tình cho phép tổng số request đồng thời tới
    // Gemini vượt quá giới hạn tài khoản thực tế.
    options.AddConcurrencyLimiter("gemini", opt =>
    {
        opt.PermitLimit = builder.Configuration.GetValue("Gemini:MaxConcurrentRequests", 5);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = builder.Configuration.GetValue("Gemini:ConcurrencyQueueLimit", 50);
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.HttpContext.Response.Headers.RetryAfter = "5";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            message = "Hệ thống đang xử lý nhiều yêu cầu tư vấn, vui lòng thử lại sau ít giây.",
        }, token);
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Không UseHttpsRedirection() — service nội bộ, TLS chỉ xử lý ở API Gateway khi deploy production.
// Không UseCors() — chỉ Gateway mới cần CORS vì browser chỉ gọi tới Gateway.
app.UseAuthentication();
app.UseAuthorization();

// Đặt SAU UseAuthorization(): request bị 401/403 không nên chiếm 1 trong số concurrency slot/
// hàng đợi dành để bảo vệ lệnh gọi Claude tốn kém phía sau.
app.UseRateLimiter();

app.MapControllers();

app.Run();

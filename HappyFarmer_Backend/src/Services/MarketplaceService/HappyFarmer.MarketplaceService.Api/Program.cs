using System.Text.Json.Serialization;
using HappyFarmer.MarketplaceService.Api.Data;
using HappyFarmer.MarketplaceService.Api.Hubs;
using HappyFarmer.MarketplaceService.Api.Services;
using HappyFarmer.Shared.Contracts.Auth;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MarketplaceDb")));

builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection(CloudinaryOptions.SectionName));
builder.Services.AddScoped<CloudinarySignatureService>();

builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AuthServiceBaseUrl"]!);
});
builder.Services.AddScoped<AuthServiceClient>();

builder.Services.AddTrustedHeaderAuthentication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/api/marketplace/hubs/chat");

app.Run();

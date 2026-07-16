using System.Net.Http.Json;
using System.Text.Json;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

public record AuthUserLookupDto(int Id, string FullName, DateTime CreatedAt, string? AvatarUrl, string? ProvinceName);

/// <summary>
/// Gọi endpoint nội bộ của Auth Service (api/auth/internal/users/lookup) để chatbot lấy tên người dùng
/// hiện tại phục vụ xưng hô cá nhân hóa. Cùng pattern với MarketplaceService/Services/AuthServiceClient.cs
/// (named HttpClient + header X-Internal-Api-Key) — không share class giữa 2 service vì mỗi service tự
/// quản lý HttpClient/config riêng.
/// </summary>
public class AuthServiceClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AuthServiceClient> logger)
{
    private const string ApiKeyHeader = "X-Internal-Api-Key";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<AuthUserLookupDto?> GetUserAsync(int userId, CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("AuthService");
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/internal/users/lookup?ids={userId}");
            request.Headers.Add(ApiKeyHeader, configuration["Internal:ApiKey"]);

            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Auth Service lookup trả về {StatusCode} cho user {UserId}", response.StatusCode, userId);
                return null;
            }

            var users = await response.Content.ReadFromJsonAsync<List<AuthUserLookupDto>>(JsonOptions, ct);
            return users?.SingleOrDefault(u => u.Id == userId);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Không gọi được Auth Service để lấy thông tin user {UserId}", userId);
            return null;
        }
    }
}

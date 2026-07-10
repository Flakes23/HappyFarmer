using System.Net.Http.Json;

namespace HappyFarmer.MarketplaceService.Api.Services;

/// <summary>
/// Gọi endpoint nội bộ của Auth Service (api/auth/internal/users/lookup) để lấy tên + ngày tham gia
/// của farmer/buyer, dùng cho yếu tố tin cậy hiển thị ở Chợ nông sản. Đây là lời gọi service-to-service
/// đầu tiên trong codebase — chỉ dùng ở luồng tạo tin (denormalize 1 lần), không gọi khi đọc, để tránh
/// N+1 và không làm luồng đọc phụ thuộc Auth Service còn sống hay không.
/// </summary>
public record UserLookupDto(int Id, string FullName, DateTime CreatedAt, string? AvatarUrl);

public class AuthServiceClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AuthServiceClient> logger)
{
    private const string ApiKeyHeader = "X-Internal-Api-Key";

    public async Task<UserLookupDto?> GetUserAsync(int userId)
    {
        try
        {
            var client = httpClientFactory.CreateClient("AuthService");
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/internal/users/lookup?ids={userId}");
            request.Headers.Add(ApiKeyHeader, configuration["Internal:ApiKey"]);

            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Auth Service lookup trả về {StatusCode} cho user {UserId}", response.StatusCode, userId);
                return null;
            }

            var users = await response.Content.ReadFromJsonAsync<List<UserLookupDto>>();
            return users?.SingleOrDefault(u => u.Id == userId);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Không gọi được Auth Service để lấy thông tin user {UserId}", userId);
            return null;
        }
    }
}

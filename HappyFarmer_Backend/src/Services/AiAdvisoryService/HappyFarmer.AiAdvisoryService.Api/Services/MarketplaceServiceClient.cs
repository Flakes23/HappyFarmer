using System.Net.Http.Json;
using System.Text.Json;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

public record MarketplaceListingDto(
    int Id, int FarmerId, string? FarmerName,
    int ProductId, decimal Quantity, string Unit, decimal PricePerUnit,
    int RegionId, string? Description, string Status, List<string> ImageUrls);

/// <summary>
/// Gọi endpoint public (AllowAnonymous) tìm tin đăng của Marketplace Service — dùng cho chatbot tìm
/// tin đăng thật qua function calling. Cùng nguyên tắc nuốt lỗi mạng/timeout như MarketPriceServiceClient.
/// </summary>
public class MarketplaceServiceClient(IHttpClientFactory httpClientFactory, ILogger<MarketplaceServiceClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);

    public async Task<List<MarketplaceListingDto>> SearchListingsAsync(
        int? productId, int? regionId, decimal? maxPrice, int pageSize, CancellationToken ct)
    {
        try
        {
            var query = new List<string> { $"pageSize={pageSize}" };
            if (productId is not null) query.Add($"productId={productId}");
            if (regionId is not null) query.Add($"regionId={regionId}");
            if (maxPrice is not null) query.Add($"maxPrice={maxPrice}");
            var url = "api/marketplace/listings?" + string.Join("&", query);

            var client = httpClientFactory.CreateClient("MarketplaceService");
            var result = await client.GetFromJsonAsync<PagedResult<MarketplaceListingDto>>(url, JsonOptions, ct);
            return result?.Items ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Không gọi được Marketplace Service để tìm tin đăng (productId={ProductId}, regionId={RegionId})", productId, regionId);
            return [];
        }
    }
}

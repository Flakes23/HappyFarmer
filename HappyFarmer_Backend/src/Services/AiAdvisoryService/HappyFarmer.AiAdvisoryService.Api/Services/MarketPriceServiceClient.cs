using System.Net.Http.Json;
using System.Text.Json;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

public record MarketPriceProductDto(int Id, string NameVi, string Unit);

public record MarketPriceRegionDto(int Id, string ProvinceName, string MarketName);

public record MarketPriceCurrentDto(int ProductId, string ProductName, int RegionId, string RegionName, decimal Price, DateOnly EffectiveDate, string? Unit);

public record MarketPriceHistoryPointDto(DateOnly EffectiveDate, decimal Price, string? Unit);

public record MarketPriceTrendingDto(
    int ProductId, string ProductName, int RegionId, string RegionName,
    decimal CurrentPrice, decimal? PreviousPrice, decimal? ChangePercent, string? Unit);

/// <summary>
/// Gọi các endpoint public (AllowAnonymous) của Market Price Service — dùng cho chatbot tra giá thật
/// qua function calling. Không cần header API key vì các endpoint này vốn công khai cho mọi client
/// (giống cách frontend gọi thẳng qua Gateway). Mỗi hàm nuốt lỗi mạng/timeout và trả list rỗng thay vì
/// throw, để tool-call của chatbot luôn có thể báo Gemini biết "không tra được" thay vì làm sập cả lượt chat.
/// </summary>
public class MarketPriceServiceClient(IHttpClientFactory httpClientFactory, ILogger<MarketPriceServiceClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);

    private HttpClient CreateClient() => httpClientFactory.CreateClient("MarketPriceService");

    public async Task<List<MarketPriceProductDto>> SearchProductsAsync(string? search, CancellationToken ct)
    {
        try
        {
            var url = string.IsNullOrWhiteSpace(search)
                ? "api/market-price/products"
                : $"api/market-price/products?search={Uri.EscapeDataString(search)}";
            var products = await CreateClient().GetFromJsonAsync<List<MarketPriceProductDto>>(url, JsonOptions, ct);
            return products ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Không gọi được Market Price Service để tìm sản phẩm {Search}", search);
            return [];
        }
    }

    public async Task<List<MarketPriceRegionDto>> GetRegionsAsync(CancellationToken ct)
    {
        try
        {
            var regions = await CreateClient().GetFromJsonAsync<List<MarketPriceRegionDto>>("api/market-price/regions", JsonOptions, ct);
            return regions ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Không gọi được Market Price Service để lấy danh sách khu vực");
            return [];
        }
    }

    /// <summary>
    /// Tra tên hiển thị theo đúng vài Id cụ thể (vd. build card cho danh sách listing vừa tìm được) —
    /// tránh tải toàn bộ catalog rồi tự lọc như GetRegionsAsync()/SearchProductsAsync(null) trước đây.
    /// </summary>
    public async Task<List<MarketPriceProductDto>> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return [];

        try
        {
            var url = "api/market-price/products/by-ids?" + string.Join("&", idList.Select(id => $"ids={id}"));
            var products = await CreateClient().GetFromJsonAsync<List<MarketPriceProductDto>>(url, JsonOptions, ct);
            return products ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Không gọi được Market Price Service để tra tên sản phẩm theo id");
            return [];
        }
    }

    public async Task<List<MarketPriceRegionDto>> GetRegionsByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return [];

        try
        {
            var url = "api/market-price/regions/by-ids?" + string.Join("&", idList.Select(id => $"ids={id}"));
            var regions = await CreateClient().GetFromJsonAsync<List<MarketPriceRegionDto>>(url, JsonOptions, ct);
            return regions ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Không gọi được Market Price Service để tra tên khu vực theo id");
            return [];
        }
    }

    public async Task<List<MarketPriceCurrentDto>> GetCurrentPricesAsync(int? productId, int? regionId, CancellationToken ct)
    {
        try
        {
            var query = new List<string>();
            if (productId is not null) query.Add($"productId={productId}");
            if (regionId is not null) query.Add($"regionId={regionId}");
            var url = "api/market-price/prices" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

            var result = await CreateClient().GetFromJsonAsync<PagedResult<MarketPriceCurrentDto>>(url, JsonOptions, ct);
            return result?.Items ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Không gọi được Market Price Service để tra giá hiện tại (productId={ProductId}, regionId={RegionId})", productId, regionId);
            return [];
        }
    }

    public async Task<List<MarketPriceHistoryPointDto>> GetPriceHistoryAsync(int productId, int? regionId, CancellationToken ct)
    {
        try
        {
            var url = $"api/market-price/prices/{productId}/history" + (regionId is not null ? $"?regionId={regionId}" : "");
            var result = await CreateClient().GetFromJsonAsync<List<MarketPriceHistoryPointDto>>(url, JsonOptions, ct);
            return result ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Không gọi được Market Price Service để lấy lịch sử giá sản phẩm {ProductId}", productId);
            return [];
        }
    }

    public async Task<List<MarketPriceTrendingDto>> GetTrendingAsync(CancellationToken ct)
    {
        try
        {
            var result = await CreateClient().GetFromJsonAsync<List<MarketPriceTrendingDto>>("api/market-price/prices/trending", JsonOptions, ct);
            return result ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Không gọi được Market Price Service để lấy top biến động giá");
            return [];
        }
    }
}

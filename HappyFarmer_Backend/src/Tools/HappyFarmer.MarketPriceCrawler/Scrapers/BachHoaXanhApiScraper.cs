using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using HappyFarmer.MarketPriceCrawler.Models;

namespace HappyFarmer.MarketPriceCrawler.Scrapers;

/// <summary>
/// Cấu hình 1 địa điểm BHX: tên tỉnh hiển thị + provinceId/storeId lấy từ yêu cầu <c>AjaxProduct</c> trong
/// DevTools sau khi đổi địa chỉ giao hàng sang tỉnh đó trên bachhoaxanh.com.
/// </summary>
public class BachHoaXanhStoreConfig
{
    public string Province { get; set; } = "";
    public int ProvinceId { get; set; }
    public int StoreId { get; set; }
}

/// <summary>
/// Lấy giá nhóm hàng của Bách Hóa Xanh cho từng cặp tỉnh/cửa hàng trong cấu hình, từ 3 nguồn trong cùng API
/// của trang rồi hợp lại, loại trùng theo mã sản phẩm:
/// <list type="number">
/// <item><c>POST /gw/Category/AjaxProduct</c> với <c>PriorityProductIds</c> rỗng (phân trang 6 sản phẩm/trang).</item>
/// <item>Cũng <c>AjaxProduct</c> nhưng kèm <c>PriorityProductIds</c> của trình duyệt (nếu có cấu hình) — máy chủ trả
/// một tập sản phẩm hơi khác lượt 1, hợp 2 lượt mới gần đủ.</item>
/// <item><c>GET /gw/Category/GetCateVegetable</c> — nhóm sản phẩm nổi bật (vd. "3 trái bắp mỹ" không có ở 2 lượt trên).</item>
/// </list>
/// Hợp 3 nguồn cho đúng số sản phẩm <c>data.total</c> trên web (đã đối chiếu với trang /cu, 43 sản phẩm).
/// API này KHÔNG phải API công bố cho bên thứ ba — BHX có thể đổi/chặn bất cứ lúc nào; chỉ bật khi tự cấu hình
/// khoá API + danh sách cửa hàng (xem README), KHÔNG bật trên VPS/CI. Gọi thưa: tối đa <see cref="MaxStores"/> cửa hàng,
/// nghỉ <see cref="DelayBetweenCalls"/> giữa các lần, gặp lỗi HTTP là dừng luôn.
/// </summary>
public class BachHoaXanhApiScraper(
    IReadOnlyList<BachHoaXanhStoreConfig> stores,
    string apiKey,
    string reverseHost,
    IReadOnlyList<int> categoryIds,
    bool useListPrice,
    IReadOnlyDictionary<int, string>? priorityProductIdsByCategory = null,
    IReadOnlyDictionary<int, string>? subCategoryByCategory = null) : IPriceSourceScraper
{
    public const string SourceNameConst = "bachhoaxanh.com";
    public string SourceName => SourceNameConst;

    private const string AjaxProductUrl = "https://api.bachhoaxanh.com/gw/Category/AjaxProduct";
    private const string FeaturedUrl = "https://api.bachhoaxanh.com/gw/Category/GetCateVegetable";
    private const int PageSize = 6;
    private const int MaxPagesPerPass = 30;
    private const int MaxStores = 10;
    private static readonly TimeSpan DelayBetweenCalls = TimeSpan.FromSeconds(3);

    private const string CategoryName = "Rau, củ, quả";
    private const string SubCategoryName = "Rau, củ, quả khác";
    private const string MarketName = "Bách Hóa Xanh";

    private int _calls;

    public async Task<List<RawPriceItem>> ScrapeAsync(HttpClient http)
    {
        var items = new List<RawPriceItem>();

        foreach (var store in stores.Take(MaxStores))
        {
            foreach (var categoryId in categoryIds)
            {
                var seen = new HashSet<long>();
                var reportedTotal = 0;
                // Nhóm con (SubCategory) gán cho sản phẩm mới của nhóm hàng này — cấu hình theo CategoryId, mặc định
                // "Rau, củ, quả khác". Sản phẩm đã tồn tại (cùng tên) giữ nhóm con cũ, server không đổi.
                var subCategory = subCategoryByCategory is not null && subCategoryByCategory.TryGetValue(categoryId, out var sub) ? sub : SubCategoryName;

                // Lượt 1: không kèm danh sách ưu tiên; lượt 2: kèm danh sách của trình duyệt cho ĐÚNG nhóm hàng này
                // (mỗi CategoryId có danh sách ưu tiên riêng — chép từ body yêu cầu AjaxProduct của nhóm đó).
                var priorityProductIds = priorityProductIdsByCategory is not null && priorityProductIdsByCategory.TryGetValue(categoryId, out var ids) ? ids : "";
                var priorityVariants = string.IsNullOrWhiteSpace(priorityProductIds)
                    ? new[] { "" }
                    : new[] { "", priorityProductIds };

                foreach (var priority in priorityVariants)
                {
                    var ok = await ScrapePagedAsync(http, store, categoryId, priority, subCategory, seen, items, total => reportedTotal = total);
                    if (!ok) return items;
                }

                // Lượt 3: nhóm sản phẩm nổi bật.
                if (!await ScrapeFeaturedAsync(http, store, categoryId, subCategory, seen, items)) return items;

                Console.WriteLine($"[{SourceName}] {store.Province} (store {store.StoreId}, cate {categoryId}): {seen.Count} sản phẩm khác nhau (web báo {reportedTotal}).");
                if (reportedTotal > 0 && seen.Count < reportedTotal)
                {
                    Console.WriteLine($"[{SourceName}]   CẢNH BÁO: thiếu {reportedTotal - seen.Count} sản phẩm so với total của API.");
                }
            }
        }

        return items;
    }

    private async Task<bool> ScrapePagedAsync(
        HttpClient http, BachHoaXanhStoreConfig store, int categoryId, string priority, string subCategory,
        HashSet<long> seen, List<RawPriceItem> items, Action<int> onTotal)
    {
        var label = string.IsNullOrEmpty(priority) ? "AjaxProduct" : "AjaxProduct+ưu tiên";
        // Trình duyệt gửi kèm mã sản phẩm cuối của trang trước (LastShowProductId) khi phân trang.
        long lastShowProductId = 0;
        var collected = 0;

        for (var page = 1; page <= MaxPagesPerPass; page++)
        {
            using var request = BuildAjaxProductRequest(store, categoryId, page, lastShowProductId, priority);
            using var doc = await SendAsync(http, request, store);
            if (doc is null) return false;

            var result = ParseProducts(doc.RootElement, store.Province, useListPrice, seen, subCategory);
            items.AddRange(result.Items);
            collected += result.ProductCount;
            if (result.Total > 0) onTotal(result.Total);
            if (result.ProductIds.Count > 0) lastShowProductId = result.ProductIds[^1];

            Console.WriteLine($"[{SourceName}]   {label} trang {page}: {result.ProductCount} sản phẩm ({result.NewProducts} mới), {result.Items.Count} mức giá (total báo: {result.Total}).");
            foreach (var reason in result.Skipped) Console.WriteLine($"[{SourceName}]     BỎ QUA: {reason}");

            if (result.ProductCount == 0 || (result.Total > 0 && collected >= result.Total)) break;
        }

        return true;
    }

    private async Task<bool> ScrapeFeaturedAsync(
        HttpClient http, BachHoaXanhStoreConfig store, int categoryId, string subCategory, HashSet<long> seen, List<RawPriceItem> items)
    {
        var url = $"{FeaturedUrl}?provinceId={store.ProvinceId}&wardId=0&districtId=0&storeId={store.StoreId}&cateId={categoryId}&customerId=0";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyHeaders(request);

        using var doc = await SendAsync(http, request, store);
        if (doc is null) return false;

        var result = ParseProducts(doc.RootElement, store.Province, useListPrice, seen, subCategory);
        items.AddRange(result.Items);
        Console.WriteLine($"[{SourceName}]   GetCateVegetable: {result.ProductCount} sản phẩm ({result.NewProducts} mới), {result.Items.Count} mức giá.");
        foreach (var reason in result.Skipped) Console.WriteLine($"[{SourceName}]     BỎ QUA: {reason}");
        return true;
    }

    /// <summary>Gửi yêu cầu (có nghỉ giữa các lần gọi); trả null nếu lỗi HTTP để dừng nguồn này, tránh bị chặn.</summary>
    private async Task<JsonDocument?> SendAsync(HttpClient http, HttpRequestMessage request, BachHoaXanhStoreConfig store)
    {
        if (_calls++ > 0) await Task.Delay(DelayBetweenCalls);

        using var response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[{SourceName}] {store.Province}: HTTP {(int)response.StatusCode} — dừng nguồn này, không thử tiếp để tránh bị chặn.");
            return null;
        }

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private HttpRequestMessage BuildAjaxProductRequest(
        BachHoaXanhStoreConfig store, int categoryId, int pageIndex, long lastShowProductId, string priority)
    {
        var body = new
        {
            provinceId = store.ProvinceId,
            wardId = 0,
            districtId = 0,
            storeId = store.StoreId,
            CategoryId = categoryId,
            SelectedBrandId = "",
            PropertyIdList = "",
            PageIndex = pageIndex,
            PageSize,
            SortStr = "",
            PriorityProductIds = priority,
            PropertySelected = Array.Empty<object>(),
            LastShowProductId = lastShowProductId,
        };

        var request = new HttpRequestMessage(HttpMethod.Post, AjaxProductUrl) { Content = JsonContent.Create(body) };
        ApplyHeaders(request);
        return request;
    }

    private void ApplyHeaders(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("accept", "application/json, text/plain, */*");
        request.Headers.TryAddWithoutValidation("origin", "https://www.bachhoaxanh.com");
        request.Headers.TryAddWithoutValidation("referer", "https://www.bachhoaxanh.com/cu");
        request.Headers.TryAddWithoutValidation("platform", "webnew");
        request.Headers.TryAddWithoutValidation("reversehost", reverseHost);
        request.Headers.TryAddWithoutValidation("xapikey", apiKey);
        request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/130 Safari/537.36");
    }

    internal record ParseResult(
        List<RawPriceItem> Items, int ProductCount, int NewProducts, int Total, List<long> ProductIds, List<string> Skipped);

    /// <summary>
    /// Đọc <c>data.products[]</c> (AjaxProduct) hoặc <c>data[]</c> (GetCateVegetable). Mỗi <c>productPrices[]</c>
    /// thành 1 RawPriceItem. Sản phẩm có mã đã nằm trong <paramref name="seenProductIds"/> (đã lấy ở lượt trước)
    /// bị bỏ qua để không trùng; mã mới được thêm vào tập này.
    /// </summary>
    internal static ParseResult ParseProducts(
        JsonElement root, string province, bool useListPrice, HashSet<long> seenProductIds, string subCategoryName = SubCategoryName)
    {
        var items = new List<RawPriceItem>();
        var productIds = new List<long>();
        var skipped = new List<string>();
        var total = 0;
        var newProducts = 0;

        if (!root.TryGetProperty("data", out var data)) return new(items, 0, 0, 0, productIds, skipped);

        JsonElement products;
        if (data.ValueKind == JsonValueKind.Array)
        {
            products = data;
        }
        else if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("products", out products) && products.ValueKind == JsonValueKind.Array)
        {
            if (data.TryGetProperty("total", out var t) && t.TryGetInt32(out var totalValue)) total = totalValue;
        }
        else
        {
            return new(items, 0, 0, 0, productIds, skipped);
        }

        var productCount = 0;
        foreach (var product in products.EnumerateArray())
        {
            productCount++;

            long? productId = product.TryGetProperty("id", out var idElement) && idElement.TryGetInt64(out var idValue) ? idValue : null;
            if (productId is not null) productIds.Add(productId.Value);

            // Đã lấy ở lượt trước (các lượt gọi trả những tập sản phẩm chồng lấn nhau) — bỏ qua cho khỏi trùng.
            if (productId is not null && !seenProductIds.Add(productId.Value)) continue;
            newProducts++;

            var name = Str(product, "name");
            if (string.IsNullOrEmpty(name))
            {
                skipped.Add($"id {productId}: không có tên");
                continue;
            }

            var bhxUnit = Str(product, "unit") ?? "";
            var canonical = Str(product, "canonical");
            var imageUrl = Str(product, "avatar");

            if (!product.TryGetProperty("productPrices", out var prices) || prices.ValueKind != JsonValueKind.Array)
            {
                skipped.Add($"\"{name}\": không có productPrices");
                continue;
            }

            var before = items.Count;
            var reasons = new List<string>();
            foreach (var priceEntry in prices.EnumerateArray())
            {
                if (priceEntry.TryGetProperty("isCanBuy", out var canBuy) && canBuy.ValueKind == JsonValueKind.False)
                {
                    reasons.Add("isCanBuy=false");
                    continue;
                }

                var priceProperty = useListPrice ? "sysPrice" : "price";
                if (!priceEntry.TryGetProperty(priceProperty, out var p) || !p.TryGetDecimal(out var price) || price <= 0)
                {
                    reasons.Add($"{priceProperty} không hợp lệ");
                    continue;
                }

                var netUnitValue = priceEntry.TryGetProperty("netUnitValue", out var nuv) && nuv.TryGetDecimal(out var v) ? v : 0m;
                var unit = BuildUnit(bhxUnit, canonical, netUnitValue);
                var productUnit = bhxUnit.Equals("Kg", StringComparison.OrdinalIgnoreCase) ? "Kg" : unit;

                items.Add(new RawPriceItem(
                    SourceNameConst, CategoryName, subCategoryName, name, productUnit,
                    province, MarketName, price, Unit: unit, ImageUrl: imageUrl));
            }

            if (items.Count == before)
            {
                skipped.Add($"\"{name}\": không lưu được mức giá nào ({(reasons.Count == 0 ? "productPrices rỗng" : string.Join("; ", reasons.Distinct()))})");
            }
        }

        return new(items, productCount, newProducts, total, productIds, skipped);
    }

    /// <summary>
    /// Giá của mặt hàng bán theo kg có nhiều quy cách (netUnitValue 0.5/1/1.5 → "500g"/"1kg"/"1.5kg"); các
    /// mặt hàng bán theo gói (Túi/Vỉ/Combo...) dùng <c>canonical</c> (khối lượng thật của gói, vd "100g") để
    /// giá so sánh được với nguồn khác — "Túi"/"Vỉ" một mình không nói lên khối lượng.
    /// </summary>
    private static string BuildUnit(string bhxUnit, string? canonical, decimal netUnitValue)
    {
        if (bhxUnit.Equals("Kg", StringComparison.OrdinalIgnoreCase) && netUnitValue > 0)
        {
            if (netUnitValue < 1)
            {
                var grams = (int)Math.Round(netUnitValue * 1000, MidpointRounding.AwayFromZero);
                return $"{grams}g";
            }

            return $"{netUnitValue.ToString("0.##", CultureInfo.InvariantCulture)}kg";
        }

        if (!string.IsNullOrWhiteSpace(canonical)) return canonical.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(bhxUnit) ? "Kg" : bhxUnit;
    }

    private static string? Str(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.String) return null;
        var text = value.GetString()?.Trim();
        return string.IsNullOrEmpty(text) ? null : text;
    }
}

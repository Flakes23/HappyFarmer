using System.Text.RegularExpressions;
using HappyFarmer.MarketPriceCrawler.Models;
using HtmlAgilityPack;

namespace HappyFarmer.MarketPriceCrawler.Scrapers;

/// <summary>
/// thucphamnhanh.com/rau-cu-qua/ — trang WooCommerce (WordPress), HTML tĩnh, không cần JS — đọc
/// thẳng qua HttpClient + HtmlAgilityPack như các nguồn HTML tĩnh trước đây, không phải dò API
/// nội bộ như BHX/WinMart. Category cha "Rau, củ, quả" cố định (đúng phạm vi trang này); mỗi sản
/// phẩm có sẵn 1-2 class "product_cat-xxx" ứng với SubCategory con (Dạng lá/Dạng củ/Dạng quả.../
/// Trái cây...) — lấy class con đầu tiên khác category cha.
/// Trang phân trang (~48 sản phẩm/trang, tổng khoảng 318) — đọc số trang thật từ dòng "Hiển thị
/// X–Y của Z kết quả" ở trang 1 thay vì hardcode, phòng khi catalog trang đổi số lượng.
/// Giá vẫn hiển thị đầy đủ dù sản phẩm hết hàng ("outofstock") — không lọc theo tồn kho, lấy
/// đúng giá niêm yết trên trang giống các nguồn khác.
/// </summary>
public class ThucPhamNhanhScraper : IPriceSourceScraper
{
    public string SourceName => "thucphamnhanh.com";

    private const string BaseUrl = "https://thucphamnhanh.com/rau-cu-qua/";
    private const string CategoryName = "Rau, củ, quả";
    private const string ProvinceName = "TP. Hồ Chí Minh";
    private const string MarketName = "Tổng hợp";

    private static readonly Dictionary<string, string> SubCategoryNames = new()
    {
        ["dang-cu"] = "Dạng củ",
        ["dang-hat"] = "Dạng hạt",
        ["dang-la"] = "Dạng lá",
        ["dang-nam"] = "Dạng nấm",
        ["dang-qua-trai"] = "Dạng quả (trái)",
        ["rau-gia-vi-tay"] = "Rau gia vị tây",
        ["rau-cu-qua-khac"] = "Rau, củ, quả khác",
        ["trai-cay"] = "Trái cây",
    };

    private static readonly Regex ResultCountRegex = new(@"của\s+([\d.,]+)\s+kết quả", RegexOptions.Compiled);
    private static readonly Regex UnitRegex = new(@"/\s*([^\d/]+)\s*$", RegexOptions.Compiled);

    // Khối lượng thật (150g, 1kg, 1 lít...) nằm trong tên chứ không phải ở đơn vị bán cạnh giá
    // (vd. "Nấm Hải Sản – Gói 150g" giá "/Gói" thực ra là 19.500đ cho 150g, không phải theo kg).
    private static readonly Regex WeightRegex = new(@"(\d+(?:[.,]\d+)?)\s*(kg|g|lít|ml)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<List<RawPriceItem>> ScrapeAsync(HttpClient http)
    {
        var items = new List<RawPriceItem>();
        var totalRows = 0;

        var firstPageHtml = await http.GetStringAsync(BaseUrl);
        var firstPageDoc = new HtmlDocument();
        firstPageDoc.LoadHtml(firstPageHtml);

        var (pageItems, pageRows) = ExtractProducts(firstPageDoc);
        items.AddRange(pageItems);
        totalRows += pageRows;

        var totalPages = CountTotalPages(firstPageDoc, pageItems.Count);
        for (var page = 2; page <= totalPages; page++)
        {
            var html = await http.GetStringAsync($"{BaseUrl}page/{page}/");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var (items2, rows2) = ExtractProducts(doc);
            items.AddRange(items2);
            totalRows += rows2;
        }

        Console.WriteLine($"[{SourceName}] Quét {totalRows} dòng trên {totalPages} trang, khớp {items.Count} sản phẩm.");
        return items;
    }

    private int CountTotalPages(HtmlDocument firstPageDoc, int itemsPerPage)
    {
        if (itemsPerPage <= 0) return 1;

        var resultCountNode = firstPageDoc.DocumentNode.SelectSingleNode("//*[contains(@class,'woocommerce-result-count')]");
        var match = resultCountNode is null ? null : ResultCountRegex.Match(resultCountNode.InnerText);
        if (match is not { Success: true }) return 1;

        var total = int.Parse(match.Groups[1].Value.Replace(".", "").Replace(",", ""));
        return (int)Math.Ceiling((double)total / itemsPerPage);
    }

    private (List<RawPriceItem> Items, int TotalRows) ExtractProducts(HtmlDocument doc)
    {
        var items = new List<RawPriceItem>();
        var totalRows = 0;

        var productNodes = doc.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' type-product ')]");
        if (productNodes is null) return (items, totalRows);

        foreach (var node in productNodes)
        {
            totalRows++;

            // InnerText không tự giải mã HTML entity dạng số (vd. "&#8211;" → "–") — phải gọi
            // DeEntitize riêng, nếu không tên sản phẩm sẽ hiện nguyên văn "&#8211;" ngoài giao diện.
            var titleNode = node.SelectSingleNode(".//p[contains(@class,'product-title')]/a");
            var title = titleNode is null ? null : HtmlEntity.DeEntitize(titleNode.InnerText).Trim();
            if (string.IsNullOrEmpty(title)) continue;

            // Hậu tố sau dấu "–" (vd. "Loại 1kg", "Gói 150g", "Loại 100g hộp") luôn chứa khối
            // lượng thật khi có — tách hẳn ra khỏi TÊN, đưa sang cột giá qua PriceEntry.Unit,
            // không nhét vào tên sản phẩm nữa. Cùng 1 mặt hàng có thể bán nhiều quy cách (vd.
            // "Đậu đỏ" có cả gói 150g lẫn túi 1kg, giá khác hẳn nhau) — nhờ vậy vẫn chung 1
            // Product "Đậu đỏ" nhưng 2 PriceEntry riêng (Unit="150g"/"1kg"), không mất giá của
            // quy cách nào.
            string? weightUnit = null;
            var dashIndex = title.LastIndexOf('–');
            if (dashIndex >= 0)
            {
                var baseName = title[..dashIndex].Trim();
                var suffix = title[(dashIndex + 1)..].Trim();
                var weightMatch = WeightRegex.Match(suffix);
                if (weightMatch.Success)
                {
                    weightUnit = $"{weightMatch.Groups[1].Value}{weightMatch.Groups[2].Value.ToLowerInvariant()}";
                    title = baseName;
                }
            }

            var priceNode = node.SelectSingleNode(".//span[contains(@class,'price')]");
            var priceText = priceNode?.InnerText ?? "";

            // Hết hàng lâu ngày thì trang không hiện số mà hiện "Liên hệ 090 1805550" (thẻ
            // <span class="amount">, KHÔNG có class "woocommerce-Price-amount" như giá thật) —
            // nếu lấy nhầm số điện thoại làm giá sẽ ra giá sai hoàn toàn (đã gặp thực tế: giá
            // "901.805.550đ" chính là số "090 1805550" bị đọc nhầm thành tiền). Chỉ tin giá lấy
            // từ đúng span "woocommerce-Price-amount", sản phẩm nào không có thì bỏ qua.
            // Sản phẩm đang giảm giá: WooCommerce bọc giá gốc trong <del>, giá hiện tại trong <ins>
            // — phải lấy đúng <ins>, nếu lấy cả priceText thì 2 số giá dính vào nhau thành 1 số sai.
            var amountNode =
                priceNode?.SelectSingleNode(".//ins//span[contains(@class,'woocommerce-Price-amount')]") ??
                priceNode?.SelectSingleNode(".//span[contains(@class,'woocommerce-Price-amount')]");
            if (amountNode is null) continue;

            var price = PriceParsing.ParseVndPrice(amountNode.InnerText);
            if (price is null || price <= 0) continue;

            var unitMatch = UnitRegex.Match(priceText);
            var unit = weightUnit ?? (unitMatch.Success ? unitMatch.Groups[1].Value.Trim() : "Kg");

            var classAttr = node.GetAttributeValue("class", "");
            var subCategorySlug = classAttr.Split(' ')
                .Where(c => c.StartsWith("product_cat-", StringComparison.Ordinal))
                .Select(c => c["product_cat-".Length..])
                .FirstOrDefault(slug => SubCategoryNames.ContainsKey(slug));
            var subCategoryName = subCategorySlug is not null ? SubCategoryNames[subCategorySlug] : "Rau, củ, quả khác";

            var imageUrl = node.SelectSingleNode(".//img")?.GetAttributeValue("src", "");
            if (string.IsNullOrEmpty(imageUrl)) imageUrl = null;

            items.Add(new RawPriceItem(
                SourceName, CategoryName, subCategoryName, title, unit,
                ProvinceName, MarketName, price.Value, Unit: weightUnit, ImageUrl: imageUrl));
        }

        return (items, totalRows);
    }
}

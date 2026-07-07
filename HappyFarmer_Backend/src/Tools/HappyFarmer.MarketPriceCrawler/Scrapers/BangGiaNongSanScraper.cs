using System.Text.RegularExpressions;
using HappyFarmer.MarketPriceCrawler.Models;
using HtmlAgilityPack;

namespace HappyFarmer.MarketPriceCrawler.Scrapers;

/// <summary>
/// banggianongsan.com công bố nông sản công nghiệp (hồ tiêu, cà phê...) theo từng tỉnh ngay
/// trong nhãn (vd. "Hồ Tiêu Gia Lai (đ/kg)") — khác thucphamdongxanh/nsvl (rau củ, chỉ 1 vùng
/// cố định). Trang cũng có nhiều dòng giá kỳ hạn quốc tế (USD/tấn, US cent/pound) — không nằm
/// trong mapping nên tự động bị bỏ qua, không cần lọc riêng theo đơn vị.
/// </summary>
public partial class BangGiaNongSanScraper : IPriceSourceScraper
{
    public string SourceName => "banggianongsan.com";

    private const string Url = "https://banggianongsan.com/";

    // Region "TP. Hồ Chí Minh" dùng chung với Region đã seed cho thucphamdongxanh.com,
    // tránh tạo thêm 1 Region trùng tỉnh chỉ khác MarketName.
    private const string HcmProvinceName = "TP. Hồ Chí Minh";
    private const string HcmMarketName = "Chợ đầu mối (tổng hợp)";

    [GeneratedRegex(@"\s*\([^)]*\)\s*$")]
    private static partial Regex UnitSuffixRegex();

    public async Task<List<RawPriceItem>> ScrapeAsync(HttpClient http)
    {
        var mapping = MappingLoader.LoadRegional(Path.Combine(AppContext.BaseDirectory, "Mapping", "banggianongsan.json"));

        var html = await http.GetStringAsync(Url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var items = new List<RawPriceItem>();
        var totalRows = 0;

        var tables = doc.DocumentNode.SelectNodes("//table[.//th[contains(text(),'Nông sản')]]");
        if (tables is not null)
        {
            foreach (var table in tables)
            {
                var rows = table.SelectNodes(".//tbody/tr");
                if (rows is null) continue;

                foreach (var row in rows)
                {
                    totalRows++;
                    var cells = row.SelectNodes("./td");
                    if (cells is null || cells.Count < 3) continue;

                    // Cột 1 dạng "Hồ Tiêu Gia Lai (đ/kg)" — bỏ phần đơn vị trong ngoặc để lấy nhãn thô.
                    var rawLabel = UnitSuffixRegex().Replace(cells[0].InnerText.Trim(), "");
                    if (!mapping.TryGetValue(rawLabel, out var mapped)) continue;

                    var price = PriceParsing.ParseVndPrice(cells[2].InnerText.Trim());
                    if (price is null || price <= 0) continue;

                    var marketName = mapped.Region == HcmProvinceName ? HcmMarketName : $"Giá tham khảo tỉnh {mapped.Region}";
                    items.Add(new RawPriceItem(SourceName, mapped.Product, mapped.Region, marketName, price.Value));
                }
            }
        }

        Console.WriteLine($"[{SourceName}] Quét {totalRows} dòng, khớp {items.Count} sản phẩm trong danh mục.");
        return items;
    }
}

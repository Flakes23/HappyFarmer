using HappyFarmer.MarketPriceCrawler.Models;
using HtmlAgilityPack;

namespace HappyFarmer.MarketPriceCrawler.Scrapers;

/// <summary>
/// thucphamdongxanh.com công bố giá là "chợ đầu mối" khu vực TP. Hồ Chí Minh, không gắn 1
/// chợ cụ thể (Bình Điền/Thủ Đức...) — dùng Region tổng hợp cho TP.HCM. Trang có 500+ mặt
/// hàng chia nhiều &lt;table&gt; (rau củ, nấm, trái cây, thực phẩm sơ chế...) cùng cấu trúc
/// &lt;th&gt;Tên hàng hóa&lt;/th&gt;; chỉ log các dòng thực sự khớp catalog để đỡ nhiễu log.
/// </summary>
public class ThucPhamDongXanhScraper : IPriceSourceScraper
{
    public string SourceName => "thucphamdongxanh.com";

    private const string Url = "https://thucphamdongxanh.com/bang-gia-rau-cu-qua-cho-dau-moi-hom-nay/";
    private const string ProvinceName = "TP. Hồ Chí Minh";
    private const string MarketName = "Chợ đầu mối (tổng hợp)";

    public async Task<List<RawPriceItem>> ScrapeAsync(HttpClient http)
    {
        var mapping = MappingLoader.LoadSimple(Path.Combine(AppContext.BaseDirectory, "Mapping", "thucphamdongxanh.json"));

        var html = await http.GetStringAsync(Url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var items = new List<RawPriceItem>();
        var totalRows = 0;

        var tables = doc.DocumentNode.SelectNodes("//table[.//th[contains(text(),'Tên hàng hóa')]]");
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
                    if (cells is null || cells.Count < 4) continue;

                    var rawLabel = cells[1].InnerText.Trim();
                    if (!mapping.TryGetValue(rawLabel, out var productName)) continue;

                    var price = PriceParsing.ParseVndPrice(cells[2].InnerText.Trim());
                    if (price is null || price <= 0) continue;

                    items.Add(new RawPriceItem(SourceName, productName, ProvinceName, MarketName, price.Value));
                }
            }
        }

        Console.WriteLine($"[{SourceName}] Quét {totalRows} dòng, khớp {items.Count} sản phẩm trong danh mục.");
        return items;
    }
}

using HappyFarmer.MarketPriceCrawler.Models;
using HtmlAgilityPack;

namespace HappyFarmer.MarketPriceCrawler.Scrapers;

/// <summary>
/// Trung tâm Khuyến nông Vĩnh Long (Sở NN&amp;PTNT tỉnh Vĩnh Long) — trang tra cứu tổng hợp
/// (TCTongHop.aspx) render sẵn dữ liệu trong HTML (không cần postback). Trang chỉ hiển thị
/// NGẪU NHIÊN một vài trong số các nhóm hàng (rau củ, trái cây, thủy sản, gia súc-gia cầm...)
/// mỗi lần tải — nhóm "Rau, Củ" không phải lúc nào cũng có mặt trong response (đã xác nhận
/// qua nhiều lần fetch trực tiếp). Retry vài lần để tăng khả năng bắt được nhóm cần dùng, thay
/// vì coi 1 lần "0 dòng" là lỗi thật.
/// Giá luôn gắn với "Chợ Vĩnh Long" cụ thể.
/// </summary>
public class NsvlScraper : IPriceSourceScraper
{
    public string SourceName => "giaca.nsvl.com.vn";

    private const string Url = "https://giaca.nsvl.com.vn/TCTongHop.aspx";
    private const string ProvinceName = "Vĩnh Long";
    private const string MarketName = "Chợ Vĩnh Long";
    private const int MaxAttempts = 5;

    public async Task<List<RawPriceItem>> ScrapeAsync(HttpClient http)
    {
        var mapping = MappingLoader.LoadSimple(Path.Combine(AppContext.BaseDirectory, "Mapping", "nsvl.json"));

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var html = await http.GetStringAsync(Url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var (items, totalRows) = Extract(doc, mapping);

            if (items.Count > 0)
            {
                Console.WriteLine($"[{SourceName}] Quét {totalRows} dòng, khớp {items.Count} sản phẩm trong danh mục (lần thử {attempt}/{MaxAttempts}).");
                return items;
            }

            if (attempt < MaxAttempts)
            {
                // Trang chỉ hiển thị ngẫu nhiên vài nhóm hàng mỗi lần tải — thử lại có thể ra nhóm khác.
                await Task.Delay(500);
            }
        }

        Console.WriteLine($"[{SourceName}] Quét {MaxAttempts} lần, nhóm \"Rau, Củ\" không xuất hiện lần nào — bỏ qua nguồn này lượt chạy này.");
        return [];
    }

    private (List<RawPriceItem> Items, int TotalRows) Extract(HtmlDocument doc, Dictionary<string, string> mapping)
    {
        var items = new List<RawPriceItem>();
        var totalRows = 0;

        var tables = doc.DocumentNode.SelectNodes("//table[.//th[contains(text(),'Tên mặt hàng')]]");
        if (tables is null) return (items, totalRows);

        foreach (var table in tables)
        {
            var rows = table.SelectNodes(".//tr[td]");
            if (rows is null) continue;

            foreach (var row in rows)
            {
                totalRows++;
                var cells = row.SelectNodes("./td");
                if (cells is null || cells.Count < 3) continue;

                var rawLabel = cells[0].InnerText.Trim();
                if (!mapping.TryGetValue(rawLabel, out var productName)) continue;

                var price = PriceParsing.ParseVndPrice(cells[2].InnerText.Trim());
                if (price is null || price <= 0) continue;

                items.Add(new RawPriceItem(SourceName, productName, ProvinceName, MarketName, price.Value));
            }
        }

        return (items, totalRows);
    }
}

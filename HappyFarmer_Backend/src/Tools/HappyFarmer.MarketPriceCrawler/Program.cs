using System.Net.Http.Json;
using System.Reflection;
using HappyFarmer.MarketPriceCrawler.Models;
using HappyFarmer.MarketPriceCrawler.Scrapers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
var configuration = builder.Configuration;

var apiBaseUrl = configuration["Api:BaseUrl"] ?? "http://localhost:5262";
var apiKey = configuration["Internal:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "CHANGE_ME_VIA_USER_SECRETS")
{
    Console.WriteLine("Thiếu Internal:ApiKey. Chạy: dotnet user-secrets set Internal:ApiKey <giá trị đã dùng cho HappyFarmer.MarketPriceService.Api>");
    return 1;
}

using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
http.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);

Console.WriteLine("== HappyFarmer Market Price Crawler ==");
Console.WriteLine($"API: {apiBaseUrl}");
Console.WriteLine();

IPriceSourceScraper[] scrapers =
[
    new ThucPhamNhanhScraper(),
];

var scraped = new List<RawPriceItem>();
foreach (var scraper in scrapers)
{
    try
    {
        scraped.AddRange(await scraper.ScrapeAsync(http));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{scraper.SourceName}] LỖI khi crawl: {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine("== Lưu từng sản phẩm lấy được (server tự tạo Category/SubCategory/Product/Region theo tên) ==");

// Cùng 1 lần crawl có thể vô tình đọc trùng đúng 1 sản phẩm (vd. trang bị tải lại khi lỗi mạng
// giữa chừng) — loại theo đúng (ProductName, Unit) để không gửi 2 bản ghi giá y hệt nhau,
// KHÔNG loại theo tên sản phẩm gốc vì mỗi tên trên trang này đã là 1 mặt hàng cụ thể, riêng biệt.
var deduped = scraped.DistinctBy(i => (i.ProductName, i.Unit)).ToList();
var effectiveDate = DateOnly.FromDateTime(DateTime.Today);
var successCount = 0;

foreach (var item in deduped)
{
    Console.WriteLine($"{item.ProductName} [{item.SubCategoryName}] @ {item.RegionProvinceName} - {item.RegionMarketName} [{item.SourceName}]: {item.Price:N0}đ/{item.ProductUnit}");

    var payload = new
    {
        item.CategoryName,
        item.SubCategoryName,
        ProductName = item.ProductName,
        item.ProductUnit,
        item.RegionProvinceName,
        item.RegionMarketName,
        item.Price,
        EffectiveDate = effectiveDate,
        item.Unit,
        item.ImageUrl,
    };

    var response = await http.PostAsJsonAsync("/api/market-price/internal/crawl-ingest", payload);
    if (response.IsSuccessStatusCode)
    {
        successCount++;
    }
    else
    {
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"  -> LỖI gửi API ({(int)response.StatusCode}): {body}");
    }
}

Console.WriteLine();
Console.WriteLine($"Hoàn tất: {successCount}/{deduped.Count} sản phẩm đã lưu vào hệ thống.");
return 0;

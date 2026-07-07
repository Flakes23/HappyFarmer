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

var catalog = await CatalogClient.LoadAsync(http);
Console.WriteLine($"Đã tải catalog: {catalog.ProductIdByName.Count} sản phẩm, {catalog.RegionIdByKey.Count} khu vực.");
Console.WriteLine();

IPriceSourceScraper[] scrapers =
[
    new NsvlScraper(),
    new ThucPhamDongXanhScraper(),
    new BangGiaNongSanScraper(),
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

var resolved = new List<ResolvedPriceItem>();
foreach (var item in scraped)
{
    if (!catalog.ProductIdByName.TryGetValue(item.ProductNameVi, out var productId))
    {
        Console.WriteLine($"[SKIP] [{item.SourceName}] Không có Product \"{item.ProductNameVi}\" trong catalog.");
        continue;
    }

    var regionKey = (item.RegionProvinceName, item.RegionMarketName);
    if (!catalog.RegionIdByKey.TryGetValue(regionKey, out var regionId))
    {
        Console.WriteLine($"[SKIP] [{item.SourceName}] Không có Region \"{item.RegionProvinceName} / {item.RegionMarketName}\" trong catalog.");
        continue;
    }

    resolved.Add(new ResolvedPriceItem(
        productId, regionId, item.ProductNameVi,
        $"{item.RegionProvinceName} - {item.RegionMarketName}", item.SourceName, item.Price));
}

Console.WriteLine();
Console.WriteLine("== Ghép nhóm theo (Sản phẩm, Khu vực) và tính giá ==");

var groups = resolved.GroupBy(r => (r.ProductId, r.RegionId)).ToList();
var effectiveDate = DateOnly.FromDateTime(DateTime.Today);
var successCount = 0;

foreach (var group in groups)
{
    var members = group.ToList();
    var price = Math.Round(members.Average(m => m.Price), 0);
    var breakdown = string.Join(", ", members.Select(m => $"{m.SourceName}={m.Price:N0}đ"));
    Console.WriteLine($"{members[0].ProductName} @ {members[0].RegionName}: [{breakdown}] -> {price:N0}đ");

    var payload = new
    {
        ProductId = group.Key.ProductId,
        RegionId = group.Key.RegionId,
        Price = price,
        EffectiveDate = effectiveDate,
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
Console.WriteLine($"Hoàn tất: {successCount}/{groups.Count} nhóm giá đã lưu vào hệ thống.");
return 0;

record ResolvedPriceItem(int ProductId, int RegionId, string ProductName, string RegionName, string SourceName, decimal Price);

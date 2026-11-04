using System.Net.Http.Json;
using System.Reflection;
using HappyFarmer.MarketPriceCrawler.Models;
using HappyFarmer.MarketPriceCrawler.Scrapers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
// File cấu hình riêng trên máy (không commit) — chứa khoá + danh sách cửa hàng nguồn BHX, xem README.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
// Trong Docker: thư mục ./crawler-config trên VPS được mount vào /app/config (xem docker-compose.prod.yml).
builder.Configuration.AddJsonFile(Path.Combine("config", "appsettings.Local.json"), optional: true, reloadOnChange: false);
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

var scrapers = new List<IPriceSourceScraper>();

// Công tắc "Crawler:EnableThucPhamNhanh" (mặc định bật) — đặt false trong appsettings.Local.json khi chỉ muốn
// thử nguồn khác mà không crawl lại thucphamnhanh.com.
if (configuration.GetValue("Crawler:EnableThucPhamNhanh", true))
{
    scrapers.Add(new ThucPhamNhanhScraper());
}
else
{
    Console.WriteLine("[thucphamnhanh.com] Đã tắt qua Crawler:EnableThucPhamNhanh=false — bỏ qua nguồn này.");
}

// Nguồn BHX gọi API trực tiếp: chỉ bật khi tự cấu hình đủ ApiKey + Stores (xem README) — không bật mặc định,
// nên chạy trên VPS/CI (không có cấu hình này) không bị ảnh hưởng.
var bhxApiKey = configuration["BachHoaXanh:ApiKey"];
var bhxStores = configuration.GetSection("BachHoaXanh:Stores").Get<List<BachHoaXanhStoreConfig>>() ?? [];
var bhxEnabled = configuration.GetValue("BachHoaXanh:Enabled", true); // đặt false (vd. env BachHoaXanh__Enabled=false) để tắt tạm
if (bhxEnabled && !string.IsNullOrWhiteSpace(bhxApiKey) && bhxStores.Count > 0)
{
    var bhxReverseHost = configuration["BachHoaXanh:ReverseHost"] ?? "http://bhxapi.live";
    var bhxCategoryIds = configuration.GetSection("BachHoaXanh:CategoryIds").Get<List<int>>() is { Count: > 0 } ids ? ids : [8785];
    var bhxUseListPrice = configuration.GetValue<bool>("BachHoaXanh:UseListPrice");
    // Chuỗi mã sản phẩm ưu tiên (cách nhau bằng dấu phẩy) RIÊNG cho từng CategoryId, lấy từ body yêu cầu
    // AjaxProduct của trình duyệt — thiếu thì máy chủ có thể không trả một số sản phẩm (xem README).
    var bhxPriorityByCategory = (configuration.GetSection("BachHoaXanh:PriorityProductIdsByCategory").Get<Dictionary<string, string>>() ?? [])
        .Where(kv => int.TryParse(kv.Key, out _))
        .ToDictionary(kv => int.Parse(kv.Key), kv => kv.Value);
    // Nhóm con (SubCategory) cho sản phẩm mới của từng CategoryId, vd. { "8820": "Dạng lá", "8788": "Trái cây" }.
    var bhxSubCategoryByCategory = (configuration.GetSection("BachHoaXanh:SubCategoryByCategory").Get<Dictionary<string, string>>() ?? [])
        .Where(kv => int.TryParse(kv.Key, out _))
        .ToDictionary(kv => int.Parse(kv.Key), kv => kv.Value);
    scrapers.Add(new BachHoaXanhApiScraper(bhxStores, bhxApiKey, bhxReverseHost, bhxCategoryIds, bhxUseListPrice, bhxPriorityByCategory, bhxSubCategoryByCategory));
}

var scraped = new List<RawPriceItem>();
var failedSources = new List<string>();
foreach (var scraper in scrapers)
{
    try
    {
        scraped.AddRange(await scraper.ScrapeAsync(http));
    }
    catch (Exception ex)
    {
        failedSources.Add(scraper.SourceName);
        Console.WriteLine($"[{scraper.SourceName}] LỖI khi crawl: {ex.Message}");
        // Lỗi mạng thường chỉ ghi chung "An error occurred while sending the request" — nguyên nhân thật (DNS, kết nối
        // bị từ chối/đặt lại, TLS, hết thời gian chờ...) nằm ở InnerException.
        for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException)
        {
            Console.WriteLine($"[{scraper.SourceName}]   ↳ {inner.GetType().Name}: {inner.Message}");
        }
    }
}

Console.WriteLine();
Console.WriteLine("== Lưu từng sản phẩm lấy được (server tự tạo Category/SubCategory/Product/Region theo tên) ==");

// Cùng 1 lần crawl có thể vô tình đọc trùng đúng 1 sản phẩm (vd. trang bị tải lại khi lỗi mạng
// giữa chừng) — loại theo (ProductName, Unit, tỉnh, chợ/nguồn) để không gửi 2 bản ghi giá y hệt nhau,
// KHÔNG loại theo tên sản phẩm gốc vì mỗi tên trên trang này đã là 1 mặt hàng cụ thể, riêng biệt; và phải
// kèm tỉnh/chợ vì cùng 1 sản phẩm ở các tỉnh khác nhau là các bản ghi giá khác nhau.
var deduped = scraped
    .DistinctBy(i => (i.ProductName, i.Unit, i.RegionProvinceName, i.RegionMarketName))
    .ToList();
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

// Có nguồn bị lỗi thì thoát mã 1 để workflow GitHub Actions báo đỏ (trước đây crawler nuốt lỗi và vẫn báo xanh
// dù không lưu được gì). Dữ liệu của các nguồn còn lại đã lưu xong ở trên.
if (failedSources.Count > 0)
{
    Console.WriteLine($"CÓ NGUỒN BỊ LỖI: {string.Join(", ", failedSources)}");
    return 1;
}

return 0;

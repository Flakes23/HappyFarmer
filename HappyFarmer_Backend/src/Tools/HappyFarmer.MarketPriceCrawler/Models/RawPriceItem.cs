namespace HappyFarmer.MarketPriceCrawler.Models;

/// <summary>
/// Mang tên thô (Category/SubCategory/Product/Region) thay vì Id — server tự find-or-create
/// theo tên khi ingest (xem InternalController.CrawlIngest bên MarketPriceService), crawler
/// không cần tải trước catalog để khớp Id.
/// </summary>
public record RawPriceItem(
    string SourceName,
    string CategoryName,
    string SubCategoryName,
    string ProductName,
    string ProductUnit,
    string RegionProvinceName,
    string RegionMarketName,
    decimal Price,
    string? Unit = null,
    string? ImageUrl = null);

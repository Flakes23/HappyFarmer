namespace HappyFarmer.MarketPriceCrawler.Models;

public record RawPriceItem(
    string SourceName,
    string ProductNameVi,
    string RegionProvinceName,
    string RegionMarketName,
    decimal Price);

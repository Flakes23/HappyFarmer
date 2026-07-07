using HappyFarmer.MarketPriceCrawler.Models;

namespace HappyFarmer.MarketPriceCrawler.Scrapers;

public interface IPriceSourceScraper
{
    string SourceName { get; }

    Task<List<RawPriceItem>> ScrapeAsync(HttpClient http);
}

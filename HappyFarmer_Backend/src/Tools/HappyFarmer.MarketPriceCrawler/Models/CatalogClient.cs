using System.Net.Http.Json;

namespace HappyFarmer.MarketPriceCrawler.Models;

public record ProductDto(int Id, string NameVi, string? Category, string Unit, string? ImageUrl);

public record RegionDto(int Id, string ProvinceName, string MarketName, double? Lat, double? Lon);

public class Catalog
{
    public required Dictionary<string, int> ProductIdByName { get; init; }
    public required Dictionary<(string ProvinceName, string MarketName), int> RegionIdByKey { get; init; }
}

public static class CatalogClient
{
    public static async Task<Catalog> LoadAsync(HttpClient http)
    {
        var products = await http.GetFromJsonAsync<List<ProductDto>>("/api/market-price/products") ?? [];
        var regions = await http.GetFromJsonAsync<List<RegionDto>>("/api/market-price/regions") ?? [];

        return new Catalog
        {
            ProductIdByName = products.ToDictionary(p => p.NameVi, p => p.Id),
            RegionIdByKey = regions.ToDictionary(r => (r.ProvinceName, r.MarketName), r => r.Id),
        };
    }
}

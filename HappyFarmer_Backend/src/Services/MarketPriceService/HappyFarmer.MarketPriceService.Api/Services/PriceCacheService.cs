using System.Text.Json;
using HappyFarmer.MarketPriceService.Api.Dtos;
using StackExchange.Redis;

namespace HappyFarmer.MarketPriceService.Api.Services;

public class PriceCacheService(IConnectionMultiplexer redis)
{
    private static readonly TimeSpan CurrentPriceTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan TrendingTtl = TimeSpan.FromMinutes(15);
    private const string TrendingKey = "market:price:trending";

    public async Task<PriceResponse?> GetCurrentPriceAsync(int productId, int regionId)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(CurrentPriceKey(productId, regionId));
        return value.HasValue ? JsonSerializer.Deserialize<PriceResponse>((string)value!) : null;
    }

    public async Task SetCurrentPriceAsync(int productId, int regionId, PriceResponse price)
    {
        var db = redis.GetDatabase();
        await db.StringSetAsync(CurrentPriceKey(productId, regionId), JsonSerializer.Serialize(price), CurrentPriceTtl);
    }

    public async Task InvalidateCurrentPriceAsync(int productId, int regionId)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync(CurrentPriceKey(productId, regionId));
        await db.KeyDeleteAsync(TrendingKey);
    }

    public async Task<List<TrendingItem>?> GetTrendingAsync()
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(TrendingKey);
        return value.HasValue ? JsonSerializer.Deserialize<List<TrendingItem>>((string)value!) : null;
    }

    public async Task SetTrendingAsync(List<TrendingItem> items)
    {
        var db = redis.GetDatabase();
        await db.StringSetAsync(TrendingKey, JsonSerializer.Serialize(items), TrendingTtl);
    }

    private static string CurrentPriceKey(int productId, int regionId) => $"market:price:current:{productId}:{regionId}";
}

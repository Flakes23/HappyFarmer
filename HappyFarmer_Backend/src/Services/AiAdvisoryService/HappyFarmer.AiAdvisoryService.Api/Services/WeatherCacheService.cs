using System.Text.Json;
using HappyFarmer.AiAdvisoryService.Api.Dtos;
using StackExchange.Redis;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

/// <summary>
/// Cache Redis cho tọa độ tỉnh/thành (không đổi, TTL dài) và dự báo thời tiết 5 ngày (đổi liên tục,
/// TTL 3 giờ) — tránh gọi lại OpenWeatherMap mỗi request. Cũng gộp 40 điểm dữ liệu 3h/lần thành
/// summary theo từng ngày để đưa gọn vào prompt Gemini.
/// </summary>
public class WeatherCacheService(IConnectionMultiplexer redis, OpenWeatherMapClient weatherClient)
{
    private static readonly TimeSpan GeoTtl = TimeSpan.FromDays(30);
    private static readonly TimeSpan ForecastTtl = TimeSpan.FromHours(3);

    public async Task<List<DailyForecastSummary>?> GetDailyForecastAsync(string provinceName)
    {
        var geo = await GetGeocodeAsync(provinceName);
        if (geo is null) return null;

        var points = await GetForecastPointsAsync(geo.Lat, geo.Lon);
        if (points is null) return null;

        return points
            .GroupBy(p => DateOnly.FromDateTime(p.DtUtc))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                // Đại diện điều kiện thời tiết cả ngày = điểm 3h nào "nghiêm trọng" nhất (giông > mưa >
                // mưa phùn > sương mù/khác > nhiều mây > quang mây) — hữu ích hơn lấy điểm đầu tiên/cuối
                // ngày, vì farmer cần biết rủi ro xấu nhất trong ngày chứ không phải điều kiện lúc nào đó.
                var representative = g.OrderBy(p => WeatherSeverityRank(p.WeatherId)).First();
                return new DailyForecastSummary(
                    g.Key,
                    g.Average(p => p.TempC),
                    g.Min(p => p.TempC),
                    g.Max(p => p.TempC),
                    g.Sum(p => p.RainMm),
                    (int)Math.Round(g.Max(p => p.Pop) * 100),
                    representative.WeatherId,
                    representative.WeatherDescription);
            })
            .ToList();
    }

    private static int WeatherSeverityRank(int weatherId) => weatherId switch
    {
        >= 200 and < 300 => 0, // Dông
        >= 600 and < 700 => 1, // Tuyết (hiếm ở VN nhưng vẫn xử lý)
        >= 500 and < 600 => 2, // Mưa
        >= 300 and < 400 => 3, // Mưa phùn
        >= 700 and < 800 => 4, // Sương mù/khói/bụi...
        > 800 and < 900 => 5, // Nhiều mây
        800 => 6, // Quang mây
        _ => 7,
    };

    private async Task<GeocodeResult?> GetGeocodeAsync(string provinceName)
    {
        var db = redis.GetDatabase();
        var key = $"geo:province:{provinceName}";

        var cached = await db.StringGetAsync(key);
        if (cached.HasValue) return JsonSerializer.Deserialize<GeocodeResult>((string)cached!);

        var geo = await weatherClient.GeocodeAsync(provinceName);
        if (geo is not null)
        {
            await db.StringSetAsync(key, JsonSerializer.Serialize(geo), GeoTtl);
        }

        return geo;
    }

    private async Task<List<ForecastPoint>?> GetForecastPointsAsync(double lat, double lon)
    {
        var db = redis.GetDatabase();
        var key = $"weather:forecast:{lat},{lon}";

        var cached = await db.StringGetAsync(key);
        if (cached.HasValue) return JsonSerializer.Deserialize<List<ForecastPoint>>((string)cached!);

        var points = await weatherClient.GetForecastAsync(lat, lon);
        if (points is not null)
        {
            await db.StringSetAsync(key, JsonSerializer.Serialize(points), ForecastTtl);
        }

        return points;
    }
}

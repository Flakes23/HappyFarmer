using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

public record GeocodeResult(double Lat, double Lon);

public record ForecastPoint(DateTime DtUtc, double TempC, double RainMm, int WeatherId, string WeatherDescription, double Pop);

/// <summary>
/// Gọi OpenWeatherMap free tier (geocoding + 5 Day/3 Hour Forecast — gói free KHÔNG có 16-day
/// daily forecast, đó là sản phẩm trả phí riêng). Cùng pattern <c>AuthServiceClient</c>: named
/// HttpClient, raw IConfiguration cho API key, bắt hẹp lỗi mạng và trả null thay vì throw.
/// </summary>
public class OpenWeatherMapClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<OpenWeatherMapClient> logger)
{
    public async Task<GeocodeResult?> GeocodeAsync(string provinceName)
    {
        try
        {
            var client = httpClientFactory.CreateClient("OpenWeatherMap");
            var apiKey = configuration["OpenWeatherMap:ApiKey"];
            var url = $"geo/1.0/direct?q={Uri.EscapeDataString(provinceName)},VN&limit=1&appid={apiKey}";

            using var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("OpenWeatherMap geocoding trả về {StatusCode} cho {Province}", response.StatusCode, provinceName);
                return null;
            }

            var results = await response.Content.ReadFromJsonAsync<List<GeoApiEntry>>();
            var first = results?.FirstOrDefault();
            return first is null ? null : new GeocodeResult(first.Lat, first.Lon);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Không gọi được OpenWeatherMap geocoding cho {Province}", provinceName);
            return null;
        }
    }

    public async Task<List<ForecastPoint>?> GetForecastAsync(double lat, double lon)
    {
        try
        {
            var client = httpClientFactory.CreateClient("OpenWeatherMap");
            var apiKey = configuration["OpenWeatherMap:ApiKey"];
            // lang=vi: OpenWeatherMap trả mô tả thời tiết (weather[].description) bằng tiếng Việt luôn,
            // khỏi phải tự map mã điều kiện sang chữ.
            var url = $"data/2.5/forecast?lat={lat}&lon={lon}&units=metric&lang=vi&appid={apiKey}";

            using var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("OpenWeatherMap forecast trả về {StatusCode} cho ({Lat},{Lon})", response.StatusCode, lat, lon);
                return null;
            }

            var body = await response.Content.ReadFromJsonAsync<ForecastApiResponse>();
            return body?.List?.Select(p => new ForecastPoint(
                DateTimeOffset.FromUnixTimeSeconds(p.Dt).UtcDateTime,
                p.Main.Temp,
                p.Rain?.ThreeHour ?? 0,
                p.Weather?.FirstOrDefault()?.Id ?? 800,
                p.Weather?.FirstOrDefault()?.Description ?? "",
                p.Pop)).ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Không gọi được OpenWeatherMap forecast cho ({Lat},{Lon})", lat, lon);
            return null;
        }
    }

    private record GeoApiEntry(string Name, double Lat, double Lon);

    private record ForecastApiResponse([property: JsonPropertyName("list")] List<ForecastApiEntry> List);

    private record ForecastApiEntry(long Dt, ForecastApiMain Main, ForecastApiRain? Rain, List<ForecastApiWeather>? Weather, double Pop);

    private record ForecastApiMain(double Temp);

    private record ForecastApiRain([property: JsonPropertyName("3h")] double ThreeHour);

    private record ForecastApiWeather(int Id, string Main, string Description);
}

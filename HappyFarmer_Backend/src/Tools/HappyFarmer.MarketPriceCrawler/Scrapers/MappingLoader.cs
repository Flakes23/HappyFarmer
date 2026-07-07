using System.Text.Json;

namespace HappyFarmer.MarketPriceCrawler.Scrapers;

public static class MappingLoader
{
    /// <summary>Mapping dạng {"nhãn thô trên trang": "NameVi chuẩn trong DB"}.</summary>
    public static Dictionary<string, string> LoadSimple(string path) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))
        ?? throw new InvalidOperationException($"Không đọc được file mapping: {path}");

    /// <summary>Mapping dạng {"nhãn thô": {"Product": "...", "Region": "..."}} — dùng cho nguồn gắn tỉnh ngay trong nhãn.</summary>
    public static Dictionary<string, (string Product, string Region)> LoadRegional(string path)
    {
        var raw = JsonSerializer.Deserialize<Dictionary<string, RegionalMappingEntry>>(File.ReadAllText(path))
            ?? throw new InvalidOperationException($"Không đọc được file mapping: {path}");

        return raw.ToDictionary(kv => kv.Key, kv => (kv.Value.Product, kv.Value.Region));
    }

    private record RegionalMappingEntry(string Product, string Region);
}

using System.Text.RegularExpressions;

namespace HappyFarmer.MarketPriceCrawler;

public static class PriceParsing
{
    /// <summary>
    /// Giá trên cả 3 trang nguồn đều là số nguyên VNĐ, dấu "." chỉ dùng ngăn cách nghìn
    /// (vd. "134.000") — bỏ hết ký tự không phải chữ số là đủ, không cần xử lý phần thập phân.
    /// </summary>
    public static decimal? ParseVndPrice(string raw)
    {
        var digitsOnly = Regex.Replace(raw, @"[^\d]", "");
        return digitsOnly.Length == 0 ? null : decimal.Parse(digitsOnly);
    }
}

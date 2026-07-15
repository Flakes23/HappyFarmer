using StackExchange.Redis;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

/// <summary>
/// Giới hạn số lượt gọi Gemini/user/ngày để kiểm soát chi phí — kiểm tra TRƯỚC khi gọi Gemini,
/// không phải sau. Tách quota theo từng tính năng (feature) vì chat và harvest-prediction có tần
/// suất sử dụng khác hẳn nhau — dùng hết quota chat không nên chặn luôn harvest-prediction và
/// ngược lại. Key: ai:ratelimit:{feature}:{userId}:{date}.
/// </summary>
public class DailyQuotaService(IConnectionMultiplexer redis)
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    /// <summary>Tăng bộ đếm và trả về true nếu user đã vượt giới hạn hôm nay cho tính năng này.</summary>
    public async Task<bool> IsOverLimitAsync(int userId, string feature, int limit)
    {
        var db = redis.GetDatabase();
        var key = QuotaKey(userId, feature);

        var newCount = await db.StringIncrementAsync(key);
        if (newCount == 1)
        {
            // Chỉ set TTL lần đầu trong ngày, tránh reset TTL mỗi lần tăng.
            await db.KeyExpireAsync(key, Ttl);
        }

        return newCount > limit;
    }

    private static string QuotaKey(int userId, string feature) => $"ai:ratelimit:{feature}:{userId}:{DateTime.UtcNow:yyyy-MM-dd}";
}

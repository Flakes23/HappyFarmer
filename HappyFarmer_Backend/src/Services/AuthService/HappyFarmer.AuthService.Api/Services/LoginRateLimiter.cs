using StackExchange.Redis;

namespace HappyFarmer.AuthService.Api.Services;

/// <summary>
/// Đếm số lần đăng nhập sai theo key (số điện thoại/IP) trong Redis — auth:ratelimit:login:{phoneOrIp}.
/// </summary>
public class LoginRateLimiter(IConnectionMultiplexer redis)
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    public async Task<bool> IsBlockedAsync(string identifier)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(Key(identifier));
        return value.HasValue && (int)value >= MaxAttempts;
    }

    public async Task RegisterFailedAttemptAsync(string identifier)
    {
        var db = redis.GetDatabase();
        var key = Key(identifier);
        var count = await db.StringIncrementAsync(key);
        if (count == 1)
        {
            await db.KeyExpireAsync(key, Window);
        }
    }

    public async Task ResetAsync(string identifier)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync(Key(identifier));
    }

    private static string Key(string identifier) => $"auth:ratelimit:login:{identifier}";
}
using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;

namespace HappyFarmer.AuthService.Api.Services;

/// <summary>
/// Lưu mã OTP quên mật khẩu trong Redis (chỉ lưu hash, có TTL):
/// auth:pwreset:otp:{email}, auth:pwreset:attempts:{email}, auth:pwreset:cooldown:{email}.
/// </summary>
public class PasswordResetOtpService(IConnectionMultiplexer redis)
{
    private const int MaxVerifyAttempts = 5;
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

    public static int OtpMinutes => (int)OtpLifetime.TotalMinutes;

    /// <summary>Trả null nếu đang trong thời gian chờ gửi lại, ngược lại trả OTP plaintext mới.</summary>
    public async Task<string?> TryCreateAsync(string email)
    {
        var db = redis.GetDatabase();
        var acquired = await db.StringSetAsync(CooldownKey(email), "1", ResendCooldown, When.NotExists);
        if (!acquired) return null;

        var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        await db.StringSetAsync(OtpKey(email), Hash(otp), OtpLifetime);
        await db.KeyDeleteAsync(AttemptsKey(email));
        return otp;
    }

    public async Task<bool> VerifyAsync(string email, string otp)
    {
        var db = redis.GetDatabase();
        var attempts = await db.StringIncrementAsync(AttemptsKey(email));
        if (attempts == 1) await db.KeyExpireAsync(AttemptsKey(email), OtpLifetime);
        if (attempts > MaxVerifyAttempts) return false;

        var stored = await db.StringGetAsync(OtpKey(email));
        if (!stored.HasValue) return false;

        var expected = Encoding.UTF8.GetBytes(stored.ToString());
        var actual = Encoding.UTF8.GetBytes(Hash(otp));
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    public async Task ConsumeAsync(string email)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync([OtpKey(email), AttemptsKey(email)]);
    }

    private static string Hash(string otp) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(otp)));
    private static string OtpKey(string email) => $"auth:pwreset:otp:{email}";
    private static string AttemptsKey(string email) => $"auth:pwreset:attempts:{email}";
    private static string CooldownKey(string email) => $"auth:pwreset:cooldown:{email}";
}

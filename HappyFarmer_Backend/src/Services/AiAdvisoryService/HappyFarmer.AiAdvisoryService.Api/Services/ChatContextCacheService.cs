using System.Text.Json;
using HappyFarmer.AiAdvisoryService.Api.Dtos;
using StackExchange.Redis;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

/// <summary>
/// Sliding-window context của 1 phiên chat, lưu trong Redis với TTL refresh mỗi lần có tin nhắn mới.
/// Khi hết TTL (>30 phút không hoạt động), controller tự fallback đọc lại từ DB (ChatMessages).
/// </summary>
public class ChatContextCacheService(IConnectionMultiplexer redis)
{
    private static readonly TimeSpan ContextTtl = TimeSpan.FromMinutes(30);
    private const int MaxTurns = 10; // ~10 lượt hội thoại = 20 entry (user + AI)

    public async Task<List<ChatTurn>?> GetContextAsync(int sessionId)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(ContextKey(sessionId));
        return value.HasValue ? JsonSerializer.Deserialize<List<ChatTurn>>((string)value!) : null;
    }

    public async Task AppendAsync(int sessionId, ChatTurn userTurn, ChatTurn aiTurn)
    {
        var db = redis.GetDatabase();
        var context = await GetContextAsync(sessionId) ?? [];
        context.Add(userTurn);
        context.Add(aiTurn);
        if (context.Count > MaxTurns * 2)
        {
            context = context[^(MaxTurns * 2)..];
        }

        await db.StringSetAsync(ContextKey(sessionId), JsonSerializer.Serialize(context), ContextTtl);
    }

    public async Task DeleteContextAsync(int sessionId)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync(ContextKey(sessionId));
    }

    private static string ContextKey(int sessionId) => $"chat:session:{sessionId}:context";
}

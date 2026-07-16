using System.Security.Claims;
using System.Text.Json;
using HappyFarmer.AiAdvisoryService.Api.Data;
using HappyFarmer.AiAdvisoryService.Api.Dtos;
using HappyFarmer.AiAdvisoryService.Api.Entities;
using HappyFarmer.AiAdvisoryService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.AiAdvisoryService.Api.Controllers;

[ApiController]
[Route("api/ai-advisory/chat")]
[Authorize]
public class ChatController(
    AiAdvisoryDbContext db,
    ChatContextCacheService contextCache,
    DailyQuotaService quota,
    GeminiChatService gemini,
    IConfiguration configuration) : ControllerBase
{
    private const int HistoryFallbackSize = 10;

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    private const int TitleMaxLength = 50;

    [HttpGet("sessions")]
    public async Task<ActionResult<List<ChatSessionSummaryDto>>> ListSessions()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var sessions = await db.ChatSessions
            .Where(s => s.FarmerId == userId.Value)
            .OrderByDescending(s => s.LastActivityAt)
            .Select(s => new ChatSessionSummaryDto(s.Id, s.Title, s.StartedAt, s.LastActivityAt, s.Status.ToString()))
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<CreateChatSessionResponse>> CreateSession()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var session = new ChatSession
        {
            FarmerId = userId.Value,
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
        };
        db.ChatSessions.Add(session);
        await db.SaveChangesAsync();

        return Ok(new CreateChatSessionResponse(session.Id, session.StartedAt));
    }

    [HttpDelete("sessions/{id:int}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var session = await db.ChatSessions.FindAsync(id);
        if (session is null || session.FarmerId != userId.Value) return NotFound();

        db.ChatSessions.Remove(session);
        await db.SaveChangesAsync();
        await contextCache.DeleteContextAsync(id);

        return NoContent();
    }

    [HttpPost("sessions/{id:int}/messages")]
    [EnableRateLimiting("gemini")]
    public async Task<ActionResult<SendChatMessageResponse>> SendMessage(int id, [FromBody] SendChatMessageRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var session = await db.ChatSessions.FindAsync(id);
        if (session is null || session.FarmerId != userId.Value) return NotFound();

        // 1. Quota/ngày (business rule) — chặn trước khi gọi Gemini để tiết kiệm chi phí.
        var chatLimit = configuration.GetValue("Gemini:DailyMessageLimitPerUser", 30);
        if (await quota.IsOverLimitAsync(userId.Value, "chat", chatLimit))
        {
            return Ok(new SendChatMessageResponse(
                id,
                "Bạn đã đạt giới hạn số tin nhắn hôm nay, vui lòng quay lại vào ngày mai.",
                DateTime.UtcNow));
        }

        // 2. Đọc sliding-window context từ Redis; fallback DB nếu hết hạn/chưa có.
        var history = await contextCache.GetContextAsync(id);
        if (history is null)
        {
            var recent = await db.ChatMessages
                .Where(m => m.SessionId == id)
                .OrderByDescending(m => m.Id)
                .Take(HistoryFallbackSize)
                .OrderBy(m => m.Id)
                .ToListAsync();
            history = recent
                .Select(m => new ChatTurn(m.Sender == ChatSender.User ? "user" : "assistant", m.Content))
                .ToList();
        }

        // 3. Gọi Gemini (đã bị giới hạn concurrency qua [EnableRateLimiting] ở trên).
        var result = await gemini.GetReplyAsync(history, request.Message, userId.Value, HttpContext.RequestAborted);
        var reply = result.Success ? result.Reply! : result.FallbackMessage!;

        // 4. Chỉ refresh Redis context khi Gemini trả lời thành công — refusal/lỗi không phải
        //    ngữ cảnh hội thoại hữu ích để replay lại cho Gemini ở lượt sau.
        if (result.Success)
        {
            await contextCache.AppendAsync(id, new ChatTurn("user", request.Message), new ChatTurn("assistant", reply));
        }

        // 5. Lưu vĩnh viễn cả 2 tin nhắn vào DB — CardsJson chỉ set khi chatbot tra được dữ liệu thật
        //    qua function-calling (giá/tin đăng), để lần sau xem lại lịch sử vẫn render đúng card.
        var cardsJson = result.Cards is { Count: > 0 } ? JsonSerializer.Serialize(result.Cards) : null;
        db.ChatMessages.Add(new ChatMessage { SessionId = id, Sender = ChatSender.User, Content = request.Message, CreatedAt = DateTime.UtcNow });
        db.ChatMessages.Add(new ChatMessage { SessionId = id, Sender = ChatSender.AI, Content = reply, CreatedAt = DateTime.UtcNow, CardsJson = cardsJson });
        session.LastActivityAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(session.Title))
        {
            var trimmed = request.Message.Trim();
            session.Title = trimmed.Length <= TitleMaxLength ? trimmed : trimmed[..TitleMaxLength].TrimEnd() + "…";
        }

        await db.SaveChangesAsync();

        return Ok(new SendChatMessageResponse(id, reply, DateTime.UtcNow, result.Cards));
    }

    [HttpGet("sessions/{id:int}/messages")]
    public async Task<ActionResult<List<ChatMessageDto>>> GetHistory(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var session = await db.ChatSessions.FindAsync(id);
        if (session is null || session.FarmerId != userId.Value) return NotFound();

        var messages = await db.ChatMessages
            .Where(m => m.SessionId == id)
            .OrderBy(m => m.Id)
            .ToListAsync();

        var dtos = messages.Select(m => new ChatMessageDto(
            m.Id, m.Sender.ToString(), m.Content, m.CreatedAt,
            m.CardsJson is null ? null : JsonSerializer.Deserialize<List<ChatCard>>(m.CardsJson)));

        return Ok(dtos);
    }
}

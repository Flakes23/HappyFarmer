using Google.GenAI;
using Google.GenAI.Types;
using HappyFarmer.AiAdvisoryService.Api.Dtos;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

public record ChatReplyResult(bool Success, string? Reply, string? FallbackMessage);

/// <summary>
/// Gọi Gemini qua Google.GenAI SDK chính thức (client-manage lịch sử hội thoại, giống pattern cũ
/// dùng cho Claude — API generateContent, không dùng Interactions API vì SDK C# chưa hỗ trợ).
/// </summary>
public class GeminiChatService(Client client, IConfiguration configuration, ILogger<GeminiChatService> logger)
{
    // Persona (giọng điệu) + ranh giới chủ đề + hướng dẫn hỏi lại khi thiếu thông tin.
    private const string SystemPrompt = """
        Bạn là một trợ lý tư vấn canh tác nông nghiệp thân thiện, kiên nhẫn, nói chuyện gần gũi bằng
        tiếng Việt như đang trò chuyện với nông dân. Tránh thuật ngữ kỹ thuật/công nghệ.

        CHỈ trả lời các câu hỏi liên quan đến nông nghiệp, canh tác, cây trồng, sâu bệnh, thời tiết
        ảnh hưởng mùa vụ, giá nông sản. Nếu người dùng hỏi chủ đề khác (chính trị, sức khỏe con người,
        lập trình, giải trí...), hãy lịch sự từ chối và gợi ý quay lại các chủ đề canh tác bạn có thể
        hỗ trợ, không cố trả lời.

        Nếu tin nhắn của người dùng chưa đủ thông tin để tư vấn chính xác (chưa rõ loại cây trồng,
        triệu chứng cụ thể, khu vực/mùa vụ), hãy hỏi lại một câu hỏi làm rõ thay vì đoán và đưa ra
        lời khuyên chung chung.
        """;

    public async Task<ChatReplyResult> GetReplyAsync(List<ChatTurn> history, string userMessage, CancellationToken ct)
    {
        var model = configuration["Gemini:Model"] ?? "gemini-3.1-flash-lite";

        var contents = history
            .Select(t => new Content
            {
                Role = t.Role == "user" ? "user" : "model",
                Parts = [new Part { Text = t.Content }],
            })
            .ToList();
        contents.Add(new Content { Role = "user", Parts = [new Part { Text = userMessage }] });

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content { Parts = [new Part { Text = SystemPrompt }] },
            MaxOutputTokens = 1024,
            // Tắt thinking — chatbot tư vấn ngắn không cần bước suy luận sâu, và thinking mặc định
            // (AUTOMATIC) là nguyên nhân gây độ trễ lớn (nhiều giây) cho một câu trả lời hội thoại đơn giản.
            ThinkingConfig = new ThinkingConfig { ThinkingBudget = 0 },
        };

        try
        {
            var response = await client.Models.GenerateContentAsync(model, contents, config, ct);

            var blockReason = response.PromptFeedback?.BlockReason;
            var candidate = response.Candidates?.FirstOrDefault();
            if (candidate is null || blockReason is not null)
            {
                logger.LogWarning("Gemini từ chối trả lời. BlockReason: {BlockReason}", blockReason);
                return new ChatReplyResult(false, null, "Xin lỗi, tôi không thể trả lời câu hỏi này. Bạn hỏi cách khác được không?");
            }

            var text = candidate.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
            return new ChatReplyResult(true, text, null);
        }
        catch (ServerError ex)
        {
            logger.LogWarning(ex, "Gemini API lỗi server");
            return new ChatReplyResult(false, null, "Dịch vụ tư vấn tạm thời gián đoạn, vui lòng thử lại sau.");
        }
        catch (ClientError ex)
        {
            logger.LogWarning(ex, "Gemini API lỗi client (rate limit/key/model)");
            return new ChatReplyResult(false, null, "Hệ thống đang bận hoặc có lỗi cấu hình, vui lòng thử lại sau ít phút.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi gọi Gemini API");
            return new ChatReplyResult(false, null, "Đã có lỗi xảy ra, vui lòng thử lại sau.");
        }
    }
}

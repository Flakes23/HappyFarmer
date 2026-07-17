using HappyFarmer.AiAdvisoryService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyFarmer.AiAdvisoryService.Api.Controllers;

public record KnowledgeIngestRequest(string SourceDocument, int ChunkIndex, string Text, string? SourceUrl);

/// <summary>
/// Endpoint nội bộ nhận chunk tài liệu RAG từ tool console <c>HappyFarmer.RagIngestor</c>
/// (src/Tools/) — xác thực bằng <c>Internal:IngestApiKey</c>, KHÁC với <c>Internal:ApiKey</c> hiện có
/// của service này (đó là key AI Advisory Service tự dùng khi GỌI RA Auth Service; cái này là key để
/// bên ngoài GỌI VÀO AI Advisory Service — 2 chiều khác nhau, tránh nhầm lẫn tên).
/// Chỉ 1 caller thật (RagIngestor tool, chạy thủ công offline) nên so sánh string đơn giản, không cần
/// dictionary nhiều key như Auth Service (nơi có ≥2 service thật sự gọi vào).
/// </summary>
[ApiController]
[Route("api/ai-advisory/internal")]
[AllowAnonymous]
public class InternalController(
    GeminiEmbeddingService embeddingService,
    QdrantKnowledgeService knowledgeService,
    IConfiguration configuration,
    ILogger<InternalController> logger) : ControllerBase
{
    private const string ApiKeyHeader = "X-Internal-Api-Key";

    [HttpPost("knowledge-ingest")]
    public async Task<IActionResult> IngestKnowledgeChunk(KnowledgeIngestRequest request, CancellationToken ct)
    {
        var expectedKey = configuration["Internal:IngestApiKey"];
        if (string.IsNullOrEmpty(expectedKey) ||
            !Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey) ||
            providedKey != expectedKey)
        {
            return Unauthorized(new { message = "API key không hợp lệ." });
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { message = "Text không được rỗng." });
        }

        try
        {
            var embedding = await embeddingService.EmbedDocumentAsync(request.Text, ct);
            await knowledgeService.UpsertChunkAsync(request.SourceDocument, request.ChunkIndex, request.Text, request.SourceUrl, embedding, ct);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi ingest chunk từ {SourceDocument} #{ChunkIndex}", request.SourceDocument, request.ChunkIndex);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Không ingest được chunk này." });
        }
    }
}

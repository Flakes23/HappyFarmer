using Google.GenAI;
using Google.GenAI.Types;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

/// <summary>
/// Wrap Gemini Embedding API (<c>EmbedContentAsync</c>) cho RAG — dùng 2 "task type" khác nhau
/// (bất đối xứng) theo đúng khuyến nghị của Gemini để tăng chất lượng retrieval: tài liệu lúc ingest
/// dùng RETRIEVAL_DOCUMENT, câu hỏi lúc chatbot tìm kiếm dùng RETRIEVAL_QUERY — cùng 1 nội dung
/// nhưng embed bằng task type sai vẫn ra vector, chỉ là độ khớp ngữ nghĩa khi so sánh sẽ kém hơn.
/// </summary>
public class GeminiEmbeddingService(Client client, IConfiguration configuration)
{
    private const int OutputDimensionality = 768;

    private string Model => configuration["Gemini:EmbeddingModel"] ?? "gemini-embedding-001";

    public Task<float[]> EmbedDocumentAsync(string text, CancellationToken ct) => EmbedAsync(text, "RETRIEVAL_DOCUMENT", ct);

    public Task<float[]> EmbedQueryAsync(string text, CancellationToken ct) => EmbedAsync(text, "RETRIEVAL_QUERY", ct);

    private async Task<float[]> EmbedAsync(string text, string taskType, CancellationToken ct)
    {
        var config = new EmbedContentConfig
        {
            TaskType = taskType,
            OutputDimensionality = OutputDimensionality,
        };

        var response = await client.Models.EmbedContentAsync(Model, text, config, ct);
        var values = response.Embeddings?.FirstOrDefault()?.Values
            ?? throw new InvalidOperationException("Gemini không trả về embedding.");

        return values.Select(v => (float)v).ToArray();
    }
}

using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

public record KnowledgeSearchResult(string SourceDocument, string Text, string? SourceUrl, float Score);

/// <summary>
/// Wrap Qdrant.Client cho RAG — 1 collection duy nhất "knowledge_chunks" lưu cả vector lẫn nội dung
/// chunk (payload), không cần bảng SQL Server song song vì payload đã thay thế hoàn toàn việc đó.
/// </summary>
public class QdrantKnowledgeService(QdrantClient qdrant, ILogger<QdrantKnowledgeService> logger)
{
    private const string CollectionName = "knowledge_chunks";
    private const int VectorSize = 768;

    public async Task EnsureCollectionExistsAsync(CancellationToken ct)
    {
        if (await qdrant.CollectionExistsAsync(CollectionName, ct))
        {
            return;
        }

        await qdrant.CreateCollectionAsync(
            CollectionName,
            new VectorParams { Size = VectorSize, Distance = Distance.Cosine },
            cancellationToken: ct);
    }

    public async Task UpsertChunkAsync(string sourceDocument, int chunkIndex, string text, string? sourceUrl, float[] embedding, CancellationToken ct)
    {
        await EnsureCollectionExistsAsync(ct);

        var point = new PointStruct
        {
            // Id tất định theo (sourceDocument, chunkIndex) — chạy lại tool ingest (vd. sau khi sửa
            // lỗi/thêm OCR cho 1 tài liệu) sẽ ghi đè đúng point cũ thay vì tạo trùng lặp.
            Id = DeterministicId(sourceDocument, chunkIndex),
            Vectors = embedding,
            Payload =
            {
                ["sourceDocument"] = sourceDocument,
                ["chunkIndex"] = chunkIndex,
                ["text"] = text,
                // Rỗng nếu tài liệu không có URL công khai (vd. bị crawler bỏ qua/không xác định
                // được link nguồn) — chatbot khi đó không chèn link, không tự bịa URL.
                ["sourceUrl"] = sourceUrl ?? "",
            },
        };

        await qdrant.UpsertAsync(CollectionName, [point], cancellationToken: ct);
    }

    private static Guid DeterministicId(string sourceDocument, int chunkIndex)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{sourceDocument}:{chunkIndex}"));
        return new Guid(hash);
    }

    public async Task<List<KnowledgeSearchResult>> SearchAsync(float[] queryEmbedding, ulong topK, CancellationToken ct)
    {
        try
        {
            var results = await qdrant.SearchAsync(
                CollectionName,
                queryEmbedding,
                limit: topK,
                payloadSelector: true,
                cancellationToken: ct);

            return results.Select(r =>
            {
                var url = r.Payload.TryGetValue("sourceUrl", out var urlValue) ? urlValue.StringValue : null;
                return new KnowledgeSearchResult(
                    r.Payload["sourceDocument"].StringValue,
                    r.Payload["text"].StringValue,
                    string.IsNullOrEmpty(url) ? null : url,
                    r.Score);
            }).ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không tìm kiếm được trong Qdrant (collection có thể chưa tồn tại/chưa có dữ liệu)");
            return [];
        }
    }
}

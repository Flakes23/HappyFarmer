using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using HappyFarmer.AiAdvisoryService.Api.Dtos;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

public record ImageDownloadResult(bool Success, byte[]? Bytes, string? MimeType, string? ErrorMessage);

public record DiseaseReplyResult(bool Success, GeminiDiseaseResult? Result, string? FallbackMessage);

/// <summary>
/// Gọi Gemini Vision để chẩn đoán bệnh cây từ ảnh — dùng structured JSON output giống
/// GeminiHarvestPredictionService, nhưng Content có thêm 1 Part ảnh (Part.FromBytes) bên cạnh
/// Part text. Ảnh được tải về từ URL Cloudinary (frontend upload thẳng lên Cloudinary trước,
/// chỉ gửi URL cho backend) chứ không nhận multipart trực tiếp.
/// </summary>
public class GeminiDiseaseDetectionService(
    Client client,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<GeminiDiseaseDetectionService> logger)
{
    private const long MaxImageBytes = 5 * 1024 * 1024;

    private const string SystemPrompt = """
        Bạn là chuyên gia bệnh học thực vật Việt Nam, giúp nông dân chẩn đoán bệnh cây trồng qua ảnh chụp.

        BƯỚC BẮT BUỘC ĐẦU TIÊN: kiểm tra ảnh được cung cấp có phải là ảnh chụp rõ nét một bộ phận cây
        trồng thật (lá, thân, quả, rễ...) hay không. Nếu ảnh mờ, chụp sai đối tượng (không phải cây),
        hoặc không đủ rõ để đánh giá — đặt isValidPlantImage=false và giải thích ngắn gọn lý do trong
        description. Vẫn phải điền đủ các field còn lại (isHealthy=false, identifiedCropType="Không xác
        định", confidenceScore=0, các danh sách để mảng rỗng []) — đây chỉ là giá trị placeholder cho đủ
        định dạng, hệ thống sẽ bỏ qua hoàn toàn khi isValidPlantImage=false.

        Nếu ảnh hợp lệ (isValidPlantImage=true): tự nhận diện loại cây trồng trong ảnh (identifiedCropType),
        rồi đánh giá cây có dấu hiệu bệnh/sâu hại hay không.
        - Nếu cây khỏe mạnh, không phát hiện bất thường: đặt isHealthy=true, diseaseName để trống, vẫn có
          thể đưa vài preventionTips chung hữu ích.
        - Nếu phát hiện bệnh/sâu hại: đặt isHealthy=false, điền diseaseName, severity ("Nhẹ"/"Trung bình"/
          "Nặng"), description giải thích triệu chứng quan sát được, và khuyến nghị xử lý cụ thể trong
          treatmentOrganic (biện pháp hữu cơ/sinh học), treatmentChemical (thuốc hóa học nếu cần thiết),
          recommendedActions (hành động ngay).

        Giọng điệu: tiếng Việt gần gũi, dễ hiểu với nông dân, tránh thuật ngữ chuyên ngành khó hiểu.
        """;

    public async Task<ImageDownloadResult> DownloadImageAsync(string imageUrl, CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ImageDownload");
            using var response = await client.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
            {
                return new ImageDownloadResult(false, null, null, "Không tải được ảnh từ đường dẫn đã cung cấp.");
            }

            var contentLength = response.Content.Headers.ContentLength;
            if (contentLength is > MaxImageBytes)
            {
                return new ImageDownloadResult(false, null, null, "Ảnh vượt quá giới hạn 5MB.");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            if (bytes.Length > MaxImageBytes)
            {
                return new ImageDownloadResult(false, null, null, "Ảnh vượt quá giới hạn 5MB.");
            }

            var mimeType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            return new ImageDownloadResult(true, bytes, mimeType, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Không tải được ảnh từ {ImageUrl}", imageUrl);
            return new ImageDownloadResult(false, null, null, "Không tải được ảnh từ đường dẫn đã cung cấp.");
        }
    }

    public async Task<DiseaseReplyResult> DetectAsync(
        byte[] imageBytes, string mimeType, string? cropTypeHint, string? note, CancellationToken ct)
    {
        var model = configuration["Gemini:Model"] ?? "gemini-3.1-flash-lite";
        var prompt = BuildPrompt(cropTypeHint, note);

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content { Parts = [new Part { Text = SystemPrompt }] },
            MaxOutputTokens = 1536,
            ThinkingConfig = new ThinkingConfig { ThinkingBudget = 0 },
            ResponseMimeType = "application/json",
            ResponseSchema = BuildResponseSchema(),
        };

        var contents = new List<Content>
        {
            new() { Role = "user", Parts = [Part.FromText(prompt), Part.FromBytes(imageBytes, mimeType)] },
        };

        try
        {
            var response = await client.Models.GenerateContentAsync(model, contents, config, ct);

            var blockReason = response.PromptFeedback?.BlockReason;
            var candidate = response.Candidates?.FirstOrDefault();
            if (candidate is null || blockReason is not null)
            {
                logger.LogWarning("Gemini từ chối chẩn đoán bệnh cây. BlockReason: {BlockReason}", blockReason);
                return new DiseaseReplyResult(false, null, "Không thể phân tích ảnh này, vui lòng kiểm tra lại hoặc thử ảnh khác.");
            }

            var json = candidate.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrWhiteSpace(json))
            {
                return new DiseaseReplyResult(false, null, "Không nhận được kết quả từ hệ thống chẩn đoán, vui lòng thử lại.");
            }

            var result = JsonSerializer.Deserialize<GeminiDiseaseResult>(json, JsonOptions);
            return result is null
                ? new DiseaseReplyResult(false, null, "Kết quả chẩn đoán không hợp lệ, vui lòng thử lại.")
                : new DiseaseReplyResult(true, result, null);
        }
        catch (ServerError ex)
        {
            logger.LogWarning(ex, "Gemini API lỗi server (disease detection)");
            return new DiseaseReplyResult(false, null, "Dịch vụ chẩn đoán tạm thời gián đoạn, vui lòng thử lại sau.");
        }
        catch (ClientError ex)
        {
            logger.LogWarning(ex, "Gemini API lỗi client (disease detection)");
            return new DiseaseReplyResult(false, null, "Hệ thống đang bận hoặc có lỗi cấu hình, vui lòng thử lại sau ít phút.");
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Không parse được JSON chẩn đoán bệnh cây từ Gemini");
            return new DiseaseReplyResult(false, null, "Kết quả chẩn đoán không hợp lệ, vui lòng thử lại.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi gọi Gemini API (disease detection)");
            return new DiseaseReplyResult(false, null, "Đã có lỗi xảy ra, vui lòng thử lại sau.");
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static string BuildPrompt(string? cropTypeHint, string? note)
    {
        var lines = new List<string> { "Hãy phân tích ảnh cây trồng được đính kèm." };

        if (!string.IsNullOrWhiteSpace(cropTypeHint))
        {
            lines.Add($"Nông dân cho biết đây có thể là cây: {cropTypeHint} (chỉ là gợi ý, hãy tự xác nhận lại từ ảnh).");
        }

        if (!string.IsNullOrWhiteSpace(note))
        {
            lines.Add($"Ghi chú thêm từ nông dân: {note}");
        }

        return string.Join("\n", lines);
    }

    private static Schema BuildResponseSchema() => new()
    {
        Type = Google.GenAI.Types.Type.Object,
        Properties = new Dictionary<string, Schema>
        {
            ["isValidPlantImage"] = new()
            {
                Type = Google.GenAI.Types.Type.Boolean,
                Description = "false nếu ảnh không phải/không đủ rõ một bộ phận cây trồng thật",
            },
            ["isHealthy"] = new() { Type = Google.GenAI.Types.Type.Boolean },
            ["identifiedCropType"] = new() { Type = Google.GenAI.Types.Type.String },
            ["diseaseName"] = new() { Type = Google.GenAI.Types.Type.String },
            ["confidenceScore"] = new() { Type = Google.GenAI.Types.Type.Number, Description = "0.0-1.0" },
            ["severity"] = new() { Type = Google.GenAI.Types.Type.String, Enum = ["Nhẹ", "Trung bình", "Nặng"] },
            ["description"] = new() { Type = Google.GenAI.Types.Type.String },
            ["treatmentOrganic"] = new() { Type = Google.GenAI.Types.Type.Array, Items = new Schema { Type = Google.GenAI.Types.Type.String } },
            ["treatmentChemical"] = new() { Type = Google.GenAI.Types.Type.Array, Items = new Schema { Type = Google.GenAI.Types.Type.String } },
            ["preventionTips"] = new() { Type = Google.GenAI.Types.Type.Array, Items = new Schema { Type = Google.GenAI.Types.Type.String } },
            ["recommendedActions"] = new() { Type = Google.GenAI.Types.Type.Array, Items = new Schema { Type = Google.GenAI.Types.Type.String } },
        },
        Required =
        [
            "isValidPlantImage", "isHealthy", "identifiedCropType", "diseaseName", "confidenceScore",
            "severity", "description", "treatmentOrganic", "treatmentChemical", "preventionTips", "recommendedActions",
        ],
    };
}

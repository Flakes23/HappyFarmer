using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using HappyFarmer.AiAdvisoryService.Api.Dtos;
using HappyFarmer.AiAdvisoryService.Api.Entities;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

public record HarvestReplyResult(bool Success, GeminiHarvestResult? Result, string? FallbackMessage);

/// <summary>
/// Gọi Gemini để suy luận thời điểm thu hoạch tối ưu — dùng structured JSON output
/// (GenerateContentConfig.ResponseSchema) thay vì text tự do như GeminiChatService, vì kết quả
/// cần parse chính xác (ngày tháng, mức độ tin cậy, danh sách rủi ro) để lưu DB.
/// </summary>
public class GeminiHarvestPredictionService(Client client, IConfiguration configuration, ILogger<GeminiHarvestPredictionService> logger)
{
    private const string SystemPrompt = """
        Bạn là chuyên gia nông nghiệp Việt Nam, giúp nông dân xác định thời điểm thu hoạch tối ưu.

        BƯỚC BẮT BUỘC ĐẦU TIÊN: kiểm tra "loại cây trồng" được cung cấp có phải là tên (tiếng Việt hoặc
        tên khoa học/quốc tế) của một loại cây nông nghiệp CÓ THẬT hay không (rau, củ, quả, ngũ cốc, cây
        công nghiệp, cây ăn trái, hoa màu...). Nếu đây là tên vô nghĩa, tên riêng, tên nhân vật/thương
        hiệu, hoặc bất kỳ thứ gì không phải một loại cây trồng thật — đặt isRecognizedCrop=false và giải
        thích ngắn gọn lý do trong reasoning — TUYỆT ĐỐI không tự bịa số liệu chu kỳ sinh trưởng cho một
        thứ không phải cây trồng thật, dù có vẻ nghe hợp lý. Vẫn phải điền đủ recommendedStartDate/
        recommendedEndDate (dùng luôn ngày hôm nay cho cả 2), confidenceLevel="Thấp", riskFactors=[] —
        đây chỉ là giá trị placeholder cho đủ định dạng, hệ thống sẽ bỏ qua hoàn toàn khi isRecognizedCrop=false.

        Nếu là cây trồng thật (isRecognizedCrop=true), dựa vào loại cây trồng, ngày trồng, khu vực, và
        (nếu có) dữ liệu thời tiết dự báo được cung cấp, hãy ước tính khoảng ngày nên thu hoạch, mức độ
        tin cậy, các rủi ro cụ thể (đặc biệt rủi ro thời tiết nếu dữ liệu dự báo nằm trong khoảng ngày thu
        hoạch dự kiến), và giải thích ngắn gọn.

        Nếu cây trồng là cây lâu năm (xoài, cà phê, hồ tiêu...), "ngày trồng" được cung cấp có thể không
        phản ánh đúng chu kỳ ra hoa/thu hoạch của vụ hiện tại — hãy tự suy luận hợp lý theo mùa vụ thông
        thường của loại cây đó tại Việt Nam thay vì tính máy móc "ngày trồng + số ngày cố định".

        Nếu KHÔNG có dữ liệu thời tiết dự báo (vì thời điểm thu hoạch dự kiến còn xa, ngoài phạm vi dự báo
        5 ngày của gói miễn phí), chỉ ước tính dựa trên chu kỳ sinh trưởng, không suy đoán rủi ro thời tiết.
        """;

    public async Task<HarvestReplyResult> PredictAsync(
        string cropType,
        DateOnly plantingDate,
        string location,
        DateOnly today,
        CropProfile? verifiedProfile,
        List<DailyForecastSummary>? weatherForecast,
        CancellationToken ct)
    {
        var model = configuration["Gemini:Model"] ?? "gemini-3.1-flash-lite";
        var prompt = BuildPrompt(cropType, plantingDate, location, today, verifiedProfile, weatherForecast);

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content { Parts = [new Part { Text = SystemPrompt }] },
            MaxOutputTokens = 1024,
            // Nhiệt độ thấp — cùng 1 loại cây/ngày trồng phải cho kết quả nhất quán giữa các lần gọi,
            // mặc định SDK (không set) để model quá "sáng tạo" khiến 2 tên gọi dân gian của cùng 1
            // loài (vd. "trâu cổ" vs "xộp" đều là Ficus pumila) ra 2 khoảng ngày thu hoạch lệch hẳn nhau.
            Temperature = 0.2,
            ThinkingConfig = new ThinkingConfig { ThinkingBudget = 0 },
            ResponseMimeType = "application/json",
            ResponseSchema = BuildResponseSchema(),
        };

        var contents = new List<Content> { new() { Role = "user", Parts = [new Part { Text = prompt }] } };

        try
        {
            var response = await client.Models.GenerateContentAsync(model, contents, config, ct);

            var blockReason = response.PromptFeedback?.BlockReason;
            var candidate = response.Candidates?.FirstOrDefault();
            if (candidate is null || blockReason is not null)
            {
                logger.LogWarning("Gemini từ chối dự đoán thu hoạch. BlockReason: {BlockReason}", blockReason);
                return new HarvestReplyResult(false, null, "Không thể đưa ra dự đoán cho yêu cầu này, vui lòng kiểm tra lại thông tin đã nhập.");
            }

            var json = candidate.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrWhiteSpace(json))
            {
                return new HarvestReplyResult(false, null, "Không nhận được kết quả từ hệ thống dự đoán, vui lòng thử lại.");
            }

            var result = JsonSerializer.Deserialize<GeminiHarvestResult>(json, JsonOptions);
            return result is null
                ? new HarvestReplyResult(false, null, "Kết quả dự đoán không hợp lệ, vui lòng thử lại.")
                : new HarvestReplyResult(true, result, null);
        }
        catch (ServerError ex)
        {
            logger.LogWarning(ex, "Gemini API lỗi server (harvest prediction)");
            return new HarvestReplyResult(false, null, "Dịch vụ dự đoán tạm thời gián đoạn, vui lòng thử lại sau.");
        }
        catch (ClientError ex)
        {
            logger.LogWarning(ex, "Gemini API lỗi client (harvest prediction)");
            return new HarvestReplyResult(false, null, "Hệ thống đang bận hoặc có lỗi cấu hình, vui lòng thử lại sau ít phút.");
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Không parse được JSON dự đoán thu hoạch từ Gemini");
            return new HarvestReplyResult(false, null, "Kết quả dự đoán không hợp lệ, vui lòng thử lại.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi gọi Gemini API (harvest prediction)");
            return new HarvestReplyResult(false, null, "Đã có lỗi xảy ra, vui lòng thử lại sau.");
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static string BuildPrompt(
        string cropType, DateOnly plantingDate, string location, DateOnly today,
        CropProfile? verifiedProfile, List<DailyForecastSummary>? weatherForecast)
    {
        var lines = new List<string>
        {
            $"Loại cây trồng: {cropType}",
            $"Ngày trồng: {plantingDate:yyyy-MM-dd}",
            $"Khu vực: {location}",
            $"Hôm nay: {today:yyyy-MM-dd}",
        };

        if (verifiedProfile is not null)
        {
            lines.Add(
                $"Dữ liệu nông học đã kiểm chứng cho {verifiedProfile.CropNameVi}: trung bình " +
                $"{verifiedProfile.AvgDaysToHarvest} ngày từ lúc trồng đến thu hoạch, nhiệt độ lý tưởng " +
                $"{verifiedProfile.IdealTempMin}-{verifiedProfile.IdealTempMax}°C, lượng mưa lý tưởng " +
                $"~{verifiedProfile.IdealRainfallMm}mm. Hãy ưu tiên dùng số liệu này thay vì tự ước tính.");
        }
        else
        {
            lines.Add("Chưa có số liệu nông học đã kiểm chứng riêng cho loại cây này — hãy tự ước tính dựa trên kiến thức nông nghiệp chung.");
        }

        if (weatherForecast is { Count: > 0 })
        {
            var forecastText = string.Join("; ", weatherForecast.Select(f =>
                $"{f.Date:yyyy-MM-dd}: {f.WeatherDescription}, nhiệt độ {f.MinTempC:F0}-{f.MaxTempC:F0}°C, " +
                $"lượng mưa {f.TotalRainfallMm:F1}mm, xác suất mưa {f.PopPercent}%"));
            lines.Add($"Dữ liệu dự báo thời tiết {weatherForecast.Count} ngày tới tại khu vực: {forecastText}.");
        }
        else
        {
            lines.Add("Không có dữ liệu dự báo thời tiết khả dụng cho khu vực/thời điểm này.");
        }

        return string.Join("\n", lines);
    }

    private static Schema BuildResponseSchema() => new()
    {
        Type = Google.GenAI.Types.Type.Object,
        Properties = new Dictionary<string, Schema>
        {
            ["isRecognizedCrop"] = new()
            {
                Type = Google.GenAI.Types.Type.Boolean,
                Description = "false nếu 'loại cây trồng' không phải tên một cây nông nghiệp có thật",
            },
            ["recommendedStartDate"] = new()
            {
                Type = Google.GenAI.Types.Type.String,
                Description = "Ngày bắt đầu nên thu hoạch, định dạng yyyy-MM-dd (placeholder = hôm nay nếu isRecognizedCrop=false)",
            },
            ["recommendedEndDate"] = new()
            {
                Type = Google.GenAI.Types.Type.String,
                Description = "Ngày kết thúc nên thu hoạch, định dạng yyyy-MM-dd (placeholder = hôm nay nếu isRecognizedCrop=false)",
            },
            ["confidenceLevel"] = new() { Type = Google.GenAI.Types.Type.String, Enum = ["Cao", "Trung bình", "Thấp"] },
            ["riskFactors"] = new() { Type = Google.GenAI.Types.Type.Array, Items = new Schema { Type = Google.GenAI.Types.Type.String } },
            ["reasoning"] = new() { Type = Google.GenAI.Types.Type.String },
        },
        Required = ["isRecognizedCrop", "recommendedStartDate", "recommendedEndDate", "confidenceLevel", "riskFactors", "reasoning"],
    };
}

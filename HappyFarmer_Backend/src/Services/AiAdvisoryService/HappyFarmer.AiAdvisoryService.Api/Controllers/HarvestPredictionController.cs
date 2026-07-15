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
[Route("api/ai-advisory/harvest-prediction")]
[Authorize]
public class HarvestPredictionController(
    AiAdvisoryDbContext db,
    DailyQuotaService quota,
    WeatherCacheService weatherCache,
    GeminiHarvestPredictionService gemini,
    IConfiguration configuration,
    ILogger<HarvestPredictionController> logger) : ControllerBase
{
    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    [HttpPost]
    [EnableRateLimiting("gemini")]
    public async Task<IActionResult> Predict([FromBody] CreateHarvestPredictionRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var limit = configuration.GetValue("Gemini:HarvestPredictionDailyLimitPerUser", 10);
        if (await quota.IsOverLimitAsync(userId.Value, "harvest", limit))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = "Bạn đã đạt giới hạn số lượt dự đoán thu hoạch hôm nay, vui lòng quay lại vào ngày mai.",
            });
        }

        // 1. Dữ liệu thời tiết (nếu geocode/forecast thất bại, coi như không có — vẫn cho Gemini
        //    tự ước tính bằng kiến thức nông học, không chặn cả tính năng vì OpenWeatherMap lỗi).
        var weatherForecast = await weatherCache.GetDailyForecastAsync(request.Location);

        // 2. Tra CropProfiles — có thì dùng số liệu đã kiểm chứng, không có thì để Gemini tự suy luận.
        var normalizedCropType = request.CropType.Trim().ToLower();
        var verifiedProfile = await db.CropProfiles
            .FirstOrDefaultAsync(c => c.CropNameVi.ToLower() == normalizedCropType);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await gemini.PredictAsync(
            request.CropType, request.PlantingDate, request.Location, today,
            verifiedProfile, weatherForecast, HttpContext.RequestAborted);

        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = result.FallbackMessage });
        }

        var prediction = result.Result!;

        // 3. Gemini tự nhận diện cropType có phải cây trồng thật không — nếu không, TỪ CHỐI hẳn thay vì
        //    lưu 1 kết quả bịa đặt (nhìn có vẻ hợp lệ nhưng vô nghĩa) vào DB.
        if (!prediction.IsRecognizedCrop)
        {
            return UnprocessableEntity(new
            {
                message = $"'{request.CropType}' không phải là tên một loại cây trồng nông nghiệp hợp lệ. " +
                           "Vui lòng kiểm tra lại và nhập đúng tên loại cây bạn đang canh tác.",
                reasoning = prediction.Reasoning,
            });
        }

        // 4. Phòng hờ: schema không ép buộc được ràng buộc "isRecognizedCrop=true thì bắt buộc có ngày"
        //    (chỉ prompt yêu cầu) — nếu Gemini lỡ bỏ trống dù nhận cây hợp lệ, coi như lỗi tạm thời thay
        //    vì NullReferenceException khi lưu DB.
        if (prediction.RecommendedStartDate is null || prediction.RecommendedEndDate is null)
        {
            logger.LogWarning("Gemini trả isRecognizedCrop=true nhưng thiếu ngày thu hoạch cho {CropType}", request.CropType);
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Không thể đưa ra dự đoán lúc này, vui lòng thử lại." });
        }

        // 5. weatherDataIncluded tính xác định trong code (không hỏi Gemini tự đánh giá) — đảm bảo
        //    nhất quán: chỉ true khi ngày bắt đầu thu hoạch dự kiến còn nằm trong khoảng đã fetch được.
        var recommendedStartDate = prediction.RecommendedStartDate.Value;
        var recommendedEndDate = prediction.RecommendedEndDate.Value;
        var weatherDataIncluded = weatherForecast is { Count: > 0 }
            && recommendedStartDate <= weatherForecast.Max(f => f.Date);

        WeatherSummaryDto? weatherSummary = null;
        if (weatherDataIncluded && weatherForecast is not null)
        {
            weatherSummary = new WeatherSummaryDto(
                weatherForecast.Average(f => f.AvgTempC),
                weatherForecast.Sum(f => f.TotalRainfallMm));
        }

        var usedVerifiedCropProfile = verifiedProfile is not null;
        var transparencyNote = BuildTransparencyNote(usedVerifiedCropProfile, weatherDataIncluded);
        var riskFactors = prediction.RiskFactors ?? [];

        var entity = new HarvestPrediction
        {
            FarmerId = userId.Value,
            CropType = request.CropType,
            PlantingDate = request.PlantingDate,
            Location = request.Location,
            RecommendedStartDate = recommendedStartDate,
            RecommendedEndDate = recommendedEndDate,
            ConfidenceLevel = prediction.ConfidenceLevel ?? "Trung bình",
            ReasoningText = prediction.Reasoning,
            RiskFactorsJson = JsonSerializer.Serialize(riskFactors),
            WeatherSummaryJson = weatherSummary is null ? null : JsonSerializer.Serialize(weatherSummary),
            UsedVerifiedCropProfile = usedVerifiedCropProfile,
            WeatherDataIncluded = weatherDataIncluded,
            CreatedAt = DateTime.UtcNow,
        };
        db.HarvestPredictions.Add(entity);
        await db.SaveChangesAsync();

        return Ok(new HarvestPredictionResponse(
            entity.Id, entity.CropType, entity.PlantingDate, entity.Location,
            entity.RecommendedStartDate, entity.RecommendedEndDate, entity.ConfidenceLevel,
            riskFactors, entity.ReasoningText, weatherSummary,
            usedVerifiedCropProfile, weatherDataIncluded, transparencyNote, entity.CreatedAt));
    }

    /// <summary>
    /// Cho FE hiện trước dự báo thời tiết ngay khi farmer chọn khu vực — không gọi Gemini, không tốn
    /// quota/ngày, chỉ đọc lại cache OpenWeatherMap (WeatherCacheService) đã dùng chung với luồng dự đoán.
    /// </summary>
    [HttpGet("weather-forecast")]
    public async Task<IActionResult> GetWeatherForecast([FromQuery] string location)
    {
        if (string.IsNullOrWhiteSpace(location)) return BadRequest(new { message = "Thiếu tham số location." });

        var forecast = await weatherCache.GetDailyForecastAsync(location);
        if (forecast is null)
        {
            return NotFound(new { message = "Không lấy được dữ liệu thời tiết cho khu vực này." });
        }

        return Ok(forecast);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<HarvestPredictionSummaryDto>>> GetHistory()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var history = await db.HarvestPredictions
            .Where(h => h.FarmerId == userId.Value)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new HarvestPredictionSummaryDto(
                h.Id, h.CropType, h.Location, h.PlantingDate, h.RecommendedStartDate, h.RecommendedEndDate, h.ConfidenceLevel, h.CreatedAt))
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var entity = await db.HarvestPredictions.FindAsync(id);
        if (entity is null || entity.FarmerId != userId.Value) return NotFound();

        var riskFactors = JsonSerializer.Deserialize<List<string>>(entity.RiskFactorsJson) ?? [];
        var weatherSummary = entity.WeatherSummaryJson is null
            ? null
            : JsonSerializer.Deserialize<WeatherSummaryDto>(entity.WeatherSummaryJson);
        var transparencyNote = BuildTransparencyNote(entity.UsedVerifiedCropProfile, entity.WeatherDataIncluded);

        return Ok(new HarvestPredictionResponse(
            entity.Id, entity.CropType, entity.PlantingDate, entity.Location,
            entity.RecommendedStartDate, entity.RecommendedEndDate, entity.ConfidenceLevel,
            riskFactors, entity.ReasoningText, weatherSummary,
            entity.UsedVerifiedCropProfile, entity.WeatherDataIncluded, transparencyNote, entity.CreatedAt));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var entity = await db.HarvestPredictions.FindAsync(id);
        if (entity is null || entity.FarmerId != userId.Value) return NotFound();

        db.HarvestPredictions.Remove(entity);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static string BuildTransparencyNote(bool usedVerifiedCropProfile, bool weatherDataIncluded) =>
        (usedVerifiedCropProfile, weatherDataIncluded) switch
        {
            (true, true) => "Dự đoán dựa trên số liệu nông học đã kiểm chứng và dữ liệu thời tiết thực tế.",
            (true, false) => "Dự đoán dựa trên số liệu nông học đã kiểm chứng. Thời điểm thu hoạch còn xa nên chưa có dữ liệu thời tiết — hỏi lại gần ngày để biết rủi ro thời tiết cụ thể.",
            (false, true) => "Dự đoán dựa trên kiến thức AI tổng quát (chưa có số liệu kiểm chứng riêng cho loại cây này), kết hợp dữ liệu thời tiết thực tế — kết quả có độ tin cậy tương đối cao",
            (false, false) => "Dự đoán dựa trên kiến thức AI tổng quát, chưa có dữ liệu thời tiết — kết quả chỉ mang tính tham khảo ban đầu.",
        };
}

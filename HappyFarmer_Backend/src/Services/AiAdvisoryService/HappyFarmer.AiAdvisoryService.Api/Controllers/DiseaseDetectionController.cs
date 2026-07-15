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
[Route("api/ai-advisory/disease-detection")]
[Authorize]
public class DiseaseDetectionController(
    AiAdvisoryDbContext db,
    DailyQuotaService quota,
    CloudinarySignatureService cloudinarySignature,
    GeminiDiseaseDetectionService gemini,
    IConfiguration configuration) : ControllerBase
{
    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    [HttpGet("cloudinary-signature")]
    public ActionResult<UploadSignatureResponse> GetCloudinarySignature() =>
        Ok(cloudinarySignature.GenerateUploadSignature());

    [HttpPost]
    [EnableRateLimiting("gemini")]
    public async Task<IActionResult> Detect([FromBody] CreateDiseaseDetectionRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var limit = configuration.GetValue("Gemini:DiseaseDetectionDailyLimitPerUser", 10);
        if (await quota.IsOverLimitAsync(userId.Value, "disease", limit))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = "Bạn đã đạt giới hạn số lượt chẩn đoán bệnh cây hôm nay, vui lòng quay lại vào ngày mai.",
            });
        }

        var download = await gemini.DownloadImageAsync(request.ImageUrl, HttpContext.RequestAborted);
        if (!download.Success)
        {
            return BadRequest(new { message = download.ErrorMessage });
        }

        var result = await gemini.DetectAsync(
            download.Bytes!, download.MimeType!, request.CropTypeHint, request.Note, HttpContext.RequestAborted);

        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = result.FallbackMessage });
        }

        var diagnosis = result.Result!;

        // Ảnh không phải cây trồng thật/không đủ rõ — từ chối hẳn, không lưu kết quả bịa đặt vào DB
        // (cùng nguyên tắc isRecognizedCrop đã áp dụng cho harvest prediction).
        if (!diagnosis.IsValidPlantImage)
        {
            return UnprocessableEntity(new
            {
                message = "Ảnh không đủ rõ hoặc không phải ảnh cây trồng. Vui lòng chụp cận cảnh bộ phận bị nghi bệnh (lá, thân, quả...) và thử lại.",
                description = diagnosis.Description,
            });
        }

        var entity = new DiseaseDetection
        {
            FarmerId = userId.Value,
            ImageUrl = request.ImageUrl,
            CropTypeHint = request.CropTypeHint,
            Note = request.Note,
            IsHealthy = diagnosis.IsHealthy,
            IdentifiedCropType = diagnosis.IdentifiedCropType,
            DiseaseName = diagnosis.DiseaseName,
            ConfidenceScore = diagnosis.ConfidenceScore,
            Severity = diagnosis.Severity,
            Description = diagnosis.Description,
            TreatmentOrganicJson = JsonSerializer.Serialize(diagnosis.TreatmentOrganic),
            TreatmentChemicalJson = JsonSerializer.Serialize(diagnosis.TreatmentChemical),
            PreventionTipsJson = JsonSerializer.Serialize(diagnosis.PreventionTips),
            RecommendedActionsJson = JsonSerializer.Serialize(diagnosis.RecommendedActions),
            CreatedAt = DateTime.UtcNow,
        };
        db.DiseaseDetections.Add(entity);
        await db.SaveChangesAsync();

        return Ok(new DiseaseDetectionResponse(
            entity.Id, entity.ImageUrl, entity.IsHealthy, entity.IdentifiedCropType, entity.DiseaseName,
            entity.ConfidenceScore, entity.Severity, entity.Description,
            diagnosis.TreatmentOrganic, diagnosis.TreatmentChemical, diagnosis.PreventionTips, diagnosis.RecommendedActions,
            entity.CreatedAt));
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<DiseaseDetectionSummaryDto>>> GetHistory()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var history = await db.DiseaseDetections
            .Where(d => d.FarmerId == userId.Value)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DiseaseDetectionSummaryDto(
                d.Id, d.ImageUrl, d.IdentifiedCropType, d.IsHealthy, d.DiseaseName, d.Severity, d.CreatedAt))
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var entity = await db.DiseaseDetections.FindAsync(id);
        if (entity is null || entity.FarmerId != userId.Value) return NotFound();

        return Ok(new DiseaseDetectionResponse(
            entity.Id, entity.ImageUrl, entity.IsHealthy, entity.IdentifiedCropType, entity.DiseaseName,
            entity.ConfidenceScore, entity.Severity, entity.Description,
            JsonSerializer.Deserialize<List<string>>(entity.TreatmentOrganicJson) ?? [],
            JsonSerializer.Deserialize<List<string>>(entity.TreatmentChemicalJson) ?? [],
            JsonSerializer.Deserialize<List<string>>(entity.PreventionTipsJson) ?? [],
            JsonSerializer.Deserialize<List<string>>(entity.RecommendedActionsJson) ?? [],
            entity.CreatedAt));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var entity = await db.DiseaseDetections.FindAsync(id);
        if (entity is null || entity.FarmerId != userId.Value) return NotFound();

        db.DiseaseDetections.Remove(entity);
        await db.SaveChangesAsync();

        return NoContent();
    }
}

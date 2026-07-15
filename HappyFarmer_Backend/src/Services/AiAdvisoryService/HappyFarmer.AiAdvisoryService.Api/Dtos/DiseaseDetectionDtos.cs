namespace HappyFarmer.AiAdvisoryService.Api.Dtos;

public record CreateDiseaseDetectionRequest(string ImageUrl, string? CropTypeHint, string? Note);

public record DiseaseDetectionResponse(
    int Id,
    string ImageUrl,
    bool IsHealthy,
    string IdentifiedCropType,
    string? DiseaseName,
    double ConfidenceScore,
    string? Severity,
    string Description,
    List<string> TreatmentOrganic,
    List<string> TreatmentChemical,
    List<string> PreventionTips,
    List<string> RecommendedActions,
    DateTime CreatedAt);

public record DiseaseDetectionSummaryDto(
    int Id,
    string ImageUrl,
    string IdentifiedCropType,
    bool IsHealthy,
    string? DiseaseName,
    string? Severity,
    DateTime CreatedAt);

/// <summary>Kết quả JSON có cấu trúc Gemini Vision trả về (qua GenerateContentConfig.ResponseSchema).</summary>
public record GeminiDiseaseResult(
    bool IsValidPlantImage,
    bool IsHealthy,
    string IdentifiedCropType,
    string? DiseaseName,
    double ConfidenceScore,
    string? Severity,
    string Description,
    List<string> TreatmentOrganic,
    List<string> TreatmentChemical,
    List<string> PreventionTips,
    List<string> RecommendedActions);

namespace HappyFarmer.AiAdvisoryService.Api.Dtos;

public record CreateHarvestPredictionRequest(string CropType, DateOnly PlantingDate, string Location);

public record WeatherSummaryDto(double AvgTempC, double TotalRainfallMm);

public record HarvestPredictionResponse(
    int Id,
    string CropType,
    DateOnly PlantingDate,
    string Location,
    DateOnly RecommendedStartDate,
    DateOnly RecommendedEndDate,
    string ConfidenceLevel,
    List<string> RiskFactors,
    string Reasoning,
    WeatherSummaryDto? WeatherSummary,
    bool UsedVerifiedCropProfile,
    bool WeatherDataIncluded,
    string TransparencyNote,
    DateTime CreatedAt);

public record HarvestPredictionSummaryDto(
    int Id,
    string CropType,
    string Location,
    DateOnly PlantingDate,
    DateOnly RecommendedStartDate,
    DateOnly RecommendedEndDate,
    string ConfidenceLevel,
    DateTime CreatedAt);

/// <summary>
/// Kết quả JSON có cấu trúc Gemini trả về (qua GenerateContentConfig.ResponseSchema). Các field trừ
/// IsRecognizedCrop/Reasoning đều nullable — Gemini để trống khi IsRecognizedCrop=false (cropType không
/// phải cây trồng thật), tránh bịa số liệu cho input vô nghĩa.
/// </summary>
public record GeminiHarvestResult(
    bool IsRecognizedCrop,
    DateOnly? RecommendedStartDate,
    DateOnly? RecommendedEndDate,
    string? ConfidenceLevel,
    List<string>? RiskFactors,
    string Reasoning);

public record DailyForecastSummary(
    DateOnly Date,
    double AvgTempC,
    double MinTempC,
    double MaxTempC,
    double TotalRainfallMm,
    int PopPercent,
    int WeatherId,
    string WeatherDescription);

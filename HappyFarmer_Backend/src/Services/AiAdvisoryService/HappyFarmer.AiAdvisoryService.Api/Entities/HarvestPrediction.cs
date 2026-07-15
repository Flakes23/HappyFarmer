namespace HappyFarmer.AiAdvisoryService.Api.Entities;

public class HarvestPrediction
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public required string CropType { get; set; }
    public DateOnly PlantingDate { get; set; }
    public required string Location { get; set; }
    public DateOnly RecommendedStartDate { get; set; }
    public DateOnly RecommendedEndDate { get; set; }
    public required string ConfidenceLevel { get; set; }
    public required string ReasoningText { get; set; }
    public required string RiskFactorsJson { get; set; }
    public string? WeatherSummaryJson { get; set; }
    public bool UsedVerifiedCropProfile { get; set; }
    public bool WeatherDataIncluded { get; set; }
    public DateTime CreatedAt { get; set; }
}

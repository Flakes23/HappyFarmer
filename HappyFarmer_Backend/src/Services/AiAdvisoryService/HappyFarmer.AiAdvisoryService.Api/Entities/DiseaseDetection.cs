namespace HappyFarmer.AiAdvisoryService.Api.Entities;

public class DiseaseDetection
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public required string ImageUrl { get; set; }
    public string? CropTypeHint { get; set; }
    public string? Note { get; set; }
    public bool IsHealthy { get; set; }
    public required string IdentifiedCropType { get; set; }
    public string? DiseaseName { get; set; }
    public double ConfidenceScore { get; set; }
    public string? Severity { get; set; }
    public required string Description { get; set; }
    public required string TreatmentOrganicJson { get; set; }
    public required string TreatmentChemicalJson { get; set; }
    public required string PreventionTipsJson { get; set; }
    public required string RecommendedActionsJson { get; set; }
    public DateTime CreatedAt { get; set; }
}

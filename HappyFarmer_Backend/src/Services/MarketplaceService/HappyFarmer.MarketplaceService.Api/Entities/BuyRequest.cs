namespace HappyFarmer.MarketplaceService.Api.Entities;

public class BuyRequest
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public DateTime? BuyerJoinedAt { get; set; }
    public string? BuyerAvatarUrl { get; set; }
    public int ProductId { get; set; }
    public decimal DesiredQuantity { get; set; }
    public required string Unit { get; set; }
    public int RegionId { get; set; }
    public decimal? MaxPricePerUnit { get; set; }
    public string? Description { get; set; }
    public BuyRequestStatus Status { get; set; } = BuyRequestStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

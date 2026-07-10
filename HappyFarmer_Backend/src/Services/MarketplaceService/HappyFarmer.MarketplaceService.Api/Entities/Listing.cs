namespace HappyFarmer.MarketplaceService.Api.Entities;

/// <summary>
/// ProductId/RegionId trỏ tới Market Price Service, FarmerId trỏ tới Auth Service — không có
/// FK thật (khác DB, khác service), chỉ lưu Id theo đúng nguyên tắc microservices (xem
/// docs/architecture/01-overview.md#2-nguyên-tắc-giao-tiếp).
/// </summary>
public class Listing
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public string? FarmerName { get; set; }
    public DateTime? FarmerJoinedAt { get; set; }
    public string? FarmerAvatarUrl { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public required string Unit { get; set; }
    public decimal PricePerUnit { get; set; }
    public int RegionId { get; set; }
    public string? Description { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public ICollection<ListingImage> Images { get; set; } = [];
}

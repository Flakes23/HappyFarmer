namespace HappyFarmer.MarketPriceService.Api.Entities;

/// <summary>
/// Append-only: "giá hiện tại" của 1 ProductId+RegionId = bản ghi Approved mới nhất
/// theo EffectiveDate, không update tại chỗ.
/// </summary>
public class PriceEntry
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int RegionId { get; set; }
    public Region Region { get; set; } = null!;
    public decimal Price { get; set; }
    public PriceSource Source { get; set; }
    public int? SubmittedByUserId { get; set; }
    public PriceEntryStatus Status { get; set; } = PriceEntryStatus.Pending;
    public DateOnly EffectiveDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

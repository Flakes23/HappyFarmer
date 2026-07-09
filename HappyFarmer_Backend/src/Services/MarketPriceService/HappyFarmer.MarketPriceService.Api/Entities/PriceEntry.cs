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

    /// <summary>
    /// Đơn vị/khối lượng thật của đúng bản ghi giá này (vd. "150g" so với "1kg" — cùng Product
    /// "Đậu đỏ" nhưng khác quy cách đóng gói, khác giá thật). Null với giá Admin/Community vì
    /// không có khái niệm "quy cách nguồn" ở đó.
    /// </summary>
    public string? Unit { get; set; }

    public PriceSource Source { get; set; }
    public int? SubmittedByUserId { get; set; }
    public PriceEntryStatus Status { get; set; } = PriceEntryStatus.Pending;
    public DateOnly EffectiveDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

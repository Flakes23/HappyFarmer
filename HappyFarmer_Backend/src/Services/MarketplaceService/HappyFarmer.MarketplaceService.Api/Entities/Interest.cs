namespace HappyFarmer.MarketplaceService.Api.Entities;

/// <summary>
/// Ghi nhận 1 lượt "quan tâm/liên hệ" giữa 2 bên, gắn với đúng 1 trong 2: một tin đăng bán
/// (ListingId) hoặc một yêu cầu mua (BuyRequestId) — bên còn lại luôn null.
/// </summary>
public class Interest
{
    public int Id { get; set; }
    public int? ListingId { get; set; }
    public Listing? Listing { get; set; }
    public int? BuyRequestId { get; set; }
    public BuyRequest? BuyRequest { get; set; }
    public int InitiatorUserId { get; set; }
    public int TargetUserId { get; set; }
    public string? Message { get; set; }
    public InterestStatus Status { get; set; } = InterestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Mốc thời gian mỗi bên (Initiator/Target) mở thread trò chuyện lần gần nhất — dùng để suy ra
    /// "chưa đọc" (so với CreatedAt của tin nhắn mới nhất) mà không cần bảng đọc riêng cho từng tin.
    /// </summary>
    public DateTime? InitiatorReadAt { get; set; }
    public DateTime? TargetReadAt { get; set; }
}

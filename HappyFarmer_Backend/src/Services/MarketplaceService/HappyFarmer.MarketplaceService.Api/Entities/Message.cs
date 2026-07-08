namespace HappyFarmer.MarketplaceService.Api.Entities;

/// <summary>
/// Một tin nhắn trong hội thoại real-time gắn với 1 Interest. Interest.Message (lời nhắn đầu tiên)
/// KHÔNG được backfill vào bảng này — API tổng hợp nó thành bubble đầu tiên khi đọc lịch sử
/// (xem MessageResponse.FromInterestSeed), Id = 0 dành riêng cho bubble tổng hợp đó (EF Identity
/// không bao giờ sinh Id = 0 nên không thể trùng với 1 Message thật).
/// </summary>
public class Message
{
    public int Id { get; set; }
    public int InterestId { get; set; }
    public Interest Interest { get; set; } = null!;
    public int SenderUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}

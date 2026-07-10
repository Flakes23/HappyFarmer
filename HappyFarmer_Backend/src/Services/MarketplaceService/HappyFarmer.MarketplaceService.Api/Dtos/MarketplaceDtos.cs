using System.ComponentModel.DataAnnotations;
using HappyFarmer.MarketplaceService.Api.Entities;

namespace HappyFarmer.MarketplaceService.Api.Dtos;

public record CreateListingRequest(
    [Required] int ProductId,
    [Required, Range(0.01, double.MaxValue)] decimal Quantity,
    [Required] string Unit,
    [Required, Range(0.01, double.MaxValue)] decimal PricePerUnit,
    [Required] int RegionId,
    string? Description,
    DateTime? ExpiresAt,
    List<string>? ImageUrls);

public record UpdateListingRequest(
    [Range(0.01, double.MaxValue)] decimal? Quantity,
    [Range(0.01, double.MaxValue)] decimal? PricePerUnit,
    string? Description,
    DateTime? ExpiresAt);

public record ListingResponse(
    int Id, int FarmerId, string? FarmerName, DateTime? FarmerJoinedAt, string? FarmerAvatarUrl, int FarmerActiveListingCount,
    int ProductId, decimal Quantity, string Unit, decimal PricePerUnit,
    int RegionId, string? Description, string Status, DateTime CreatedAt, DateTime? ExpiresAt,
    List<string> ImageUrls)
{
    public static ListingResponse FromEntity(Listing l, int farmerActiveListingCount = 0) => new(
        l.Id, l.FarmerId, l.FarmerName, l.FarmerJoinedAt, l.FarmerAvatarUrl, farmerActiveListingCount,
        l.ProductId, l.Quantity, l.Unit, l.PricePerUnit,
        l.RegionId, l.Description, l.Status.ToString(), l.CreatedAt, l.ExpiresAt,
        l.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList());
}

public record CreateBuyRequestRequest(
    [Required] int ProductId,
    [Required, Range(0.01, double.MaxValue)] decimal DesiredQuantity,
    [Required] string Unit,
    [Required] int RegionId,
    [Range(0.01, double.MaxValue)] decimal? MaxPricePerUnit,
    string? Description);

public record BuyRequestResponse(
    int Id, int BuyerId, string? BuyerName, DateTime? BuyerJoinedAt, string? BuyerAvatarUrl, int BuyerActiveBuyRequestCount,
    int ProductId, decimal DesiredQuantity, string Unit, int RegionId,
    decimal? MaxPricePerUnit, string? Description, string Status, DateTime CreatedAt)
{
    public static BuyRequestResponse FromEntity(BuyRequest br, int buyerActiveBuyRequestCount = 0) => new(
        br.Id, br.BuyerId, br.BuyerName, br.BuyerJoinedAt, br.BuyerAvatarUrl, buyerActiveBuyRequestCount,
        br.ProductId, br.DesiredQuantity, br.Unit, br.RegionId,
        br.MaxPricePerUnit, br.Description, br.Status.ToString(), br.CreatedAt);
}

public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);

public record ContactListingRequest(string? Message);

public record InterestListingSummary(
    int ProductId, decimal Quantity, string Unit, decimal PricePerUnit, string Status, string? ImageUrl)
{
    public static InterestListingSummary FromEntity(Listing l) => new(
        l.ProductId, l.Quantity, l.Unit, l.PricePerUnit, l.Status.ToString(),
        l.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault());
}

public record InterestBuyRequestSummary(
    int ProductId, decimal DesiredQuantity, string Unit, decimal? MaxPricePerUnit, string Status)
{
    public static InterestBuyRequestSummary FromEntity(BuyRequest br) => new(
        br.ProductId, br.DesiredQuantity, br.Unit, br.MaxPricePerUnit, br.Status.ToString());
}

public record InterestLastMessage(string Body, int SenderUserId, DateTime CreatedAt);

public record InterestResponse(
    int Id, int? ListingId, InterestListingSummary? Listing, int? BuyRequestId, InterestBuyRequestSummary? BuyRequest,
    int InitiatorUserId, int TargetUserId, string? Message, string Status, DateTime CreatedAt, bool HasUnread,
    InterestLastMessage? LastMessage)
{
    public static InterestResponse FromEntity(Interest i, int currentUserId, Message? lastMessage) => new(
        i.Id, i.ListingId, i.Listing is null ? null : InterestListingSummary.FromEntity(i.Listing),
        i.BuyRequestId, i.BuyRequest is null ? null : InterestBuyRequestSummary.FromEntity(i.BuyRequest),
        i.InitiatorUserId, i.TargetUserId, i.Message, i.Status.ToString(), i.CreatedAt,
        ComputeHasUnread(i, currentUserId, lastMessage?.CreatedAt),
        // Chưa có tin nhắn thật nào trong bảng Messages (mới vừa liên hệ) thì fallback về lời nhắn
        // mở đầu (Interest.Message) — luôn hiện được preview kể cả khi đối phương chưa trả lời.
        lastMessage is not null
            ? new InterestLastMessage(lastMessage.Body, lastMessage.SenderUserId, lastMessage.CreatedAt)
            : (i.Message is not null ? new InterestLastMessage(i.Message, i.InitiatorUserId, i.CreatedAt) : null));

    /// <summary>
    /// "Chưa đọc" = mốc đọc gần nhất của người đang xem (Initiator/TargetReadAt) cũ hơn hoạt động
    /// gần nhất của cuộc trò chuyện (tin nhắn mới nhất, hoặc CreatedAt nếu chưa ai nhắn thêm gì).
    /// </summary>
    public static bool ComputeHasUnread(Interest i, int currentUserId, DateTime? lastMessageAt)
    {
        var lastActivity = lastMessageAt ?? i.CreatedAt;
        var myReadAt = currentUserId == i.InitiatorUserId ? i.InitiatorReadAt : i.TargetReadAt;
        return myReadAt is null || myReadAt < lastActivity;
    }
}

public record UnreadCountResponse(int Count);

public record SendMessageRequest([Required, MinLength(1), MaxLength(4000)] string Body);

public record MessageResponse(
    int Id, int InterestId, int SenderUserId, string Body, DateTime CreatedAt, DateTime? ReadAt)
{
    public static MessageResponse FromEntity(Message m) => new(
        m.Id, m.InterestId, m.SenderUserId, m.Body, m.CreatedAt, m.ReadAt);

    /// <summary>
    /// Bubble tổng hợp từ Interest.Message (lời nhắn đầu tiên, chưa từng nằm trong bảng Messages).
    /// Id luôn là 0 — không bao giờ trùng với 1 Message thật (EF Identity không sinh Id = 0).
    /// </summary>
    public static MessageResponse FromInterestSeed(Interest i) => new(
        0, i.Id, i.InitiatorUserId, i.Message ?? string.Empty, i.CreatedAt, null);
}

public record MessageHistoryResponse(List<MessageResponse> Messages, bool HasMore);

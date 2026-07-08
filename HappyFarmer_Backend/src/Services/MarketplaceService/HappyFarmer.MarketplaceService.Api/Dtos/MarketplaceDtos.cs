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
    int Id, int FarmerId, string? FarmerName, DateTime? FarmerJoinedAt, int FarmerActiveListingCount,
    int ProductId, decimal Quantity, string Unit, decimal PricePerUnit,
    int RegionId, string? Description, string Status, DateTime CreatedAt, DateTime? ExpiresAt,
    List<string> ImageUrls)
{
    public static ListingResponse FromEntity(Listing l, int farmerActiveListingCount = 0) => new(
        l.Id, l.FarmerId, l.FarmerName, l.FarmerJoinedAt, farmerActiveListingCount,
        l.ProductId, l.Quantity, l.Unit, l.PricePerUnit,
        l.RegionId, l.Description, l.Status.ToString(), l.CreatedAt, l.ExpiresAt,
        l.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList());
}

public record CreateBuyRequestRequest(
    [Required] int ProductId,
    [Required, Range(0.01, double.MaxValue)] decimal DesiredQuantity,
    [Required] int RegionId,
    [Range(0.01, double.MaxValue)] decimal? MaxPricePerUnit,
    string? Description);

public record BuyRequestResponse(
    int Id, int BuyerId, string? BuyerName, DateTime? BuyerJoinedAt, int BuyerActiveBuyRequestCount,
    int ProductId, decimal DesiredQuantity, int RegionId,
    decimal? MaxPricePerUnit, string? Description, string Status, DateTime CreatedAt)
{
    public static BuyRequestResponse FromEntity(BuyRequest br, int buyerActiveBuyRequestCount = 0) => new(
        br.Id, br.BuyerId, br.BuyerName, br.BuyerJoinedAt, buyerActiveBuyRequestCount,
        br.ProductId, br.DesiredQuantity, br.RegionId,
        br.MaxPricePerUnit, br.Description, br.Status.ToString(), br.CreatedAt);
}

public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);

public record ContactListingRequest(string? Message);

public record InterestResponse(
    int Id, int? ListingId, int? BuyRequestId, int InitiatorUserId, int TargetUserId,
    string? Message, string Status, DateTime CreatedAt)
{
    public static InterestResponse FromEntity(Interest i) => new(
        i.Id, i.ListingId, i.BuyRequestId, i.InitiatorUserId, i.TargetUserId,
        i.Message, i.Status.ToString(), i.CreatedAt);
}

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

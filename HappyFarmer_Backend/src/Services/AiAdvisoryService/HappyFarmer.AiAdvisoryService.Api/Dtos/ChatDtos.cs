using System.Text.Json.Serialization;

namespace HappyFarmer.AiAdvisoryService.Api.Dtos;

public record CreateChatSessionResponse(int SessionId, DateTime StartedAt);

public record ChatSessionSummaryDto(int Id, string? Title, DateTime StartedAt, DateTime LastActivityAt, string Status);

public record SendChatMessageRequest(string Message);

public record SendChatMessageResponse(int SessionId, string Reply, DateTime Timestamp, List<ChatCard>? Cards = null);

public record ChatMessageDto(int Id, string Sender, string Content, DateTime CreatedAt, List<ChatCard>? Cards = null);

/// <summary>
/// Card hiển thị trong khung chat khi chatbot dùng function-calling tra được dữ liệu thật (giá/tin
/// đăng). Backend tự build từ dữ liệu thô gọi được (Market Price/Marketplace Service), không phụ
/// thuộc Gemini format JSON. Discriminator "type" để frontend switch UI đúng loại card.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PriceCard), typeDiscriminator: "price")]
[JsonDerivedType(typeof(ListingCard), typeDiscriminator: "listing")]
public abstract record ChatCard;

public record PriceCard(
    int ProductId, string ProductName, string RegionName,
    decimal CurrentPrice, decimal? ChangePercent, string? Unit, string Url) : ChatCard;

public record ListingCard(
    int ListingId, string ProductName, string RegionName,
    decimal PricePerUnit, decimal Quantity, string Unit,
    string? ImageUrl, string? FarmerName, string Url) : ChatCard;

/// <summary>
/// Một lượt hội thoại trong sliding-window context (Redis) — Role là "user" hoặc "assistant",
/// khớp với Role của Anthropic SDK khi build lại messages gửi cho Claude.
/// </summary>
public record ChatTurn(string Role, string Content);

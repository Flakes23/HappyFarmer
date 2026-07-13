namespace HappyFarmer.Shared.Contracts.Events;

/// <summary>
/// Publish khi FullName/AvatarUrl của user thay đổi (topic auth.user-updated.v1) — Marketplace
/// Service subscribe để đồng bộ lại FarmerName/BuyerName/FarmerAvatarUrl/BuyerAvatarUrl đã
/// denormalize. Kafka at-least-once, không exactly-once — nhưng việc ghi đè cùng giá trị nhiều
/// lần là vô hại (idempotent tự nhiên) nên EventId không cần lưu lại để dedupe, chỉ giữ cho
/// đúng envelope convention chung với các topic khác (xem docs/architecture/01-overview.md).
/// </summary>
public record UserProfileUpdatedEvent(
    Guid EventId,
    int UserId,
    string FullName,
    string? AvatarUrl,
    DateTime OccurredAt);

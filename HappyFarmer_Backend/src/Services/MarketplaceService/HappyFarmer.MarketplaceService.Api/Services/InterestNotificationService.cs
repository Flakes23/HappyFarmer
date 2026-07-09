using HappyFarmer.MarketplaceService.Api.Data;
using HappyFarmer.MarketplaceService.Api.Dtos;
using HappyFarmer.MarketplaceService.Api.Entities;
using HappyFarmer.MarketplaceService.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketplaceService.Api.Services;

/// <summary>
/// Tính "chưa đọc"/tin nhắn cuối (dùng chung bởi InterestsController) và đẩy realtime qua SignalR
/// tới đúng group riêng của user (ChatHub.UserGroupName) mỗi khi có tin nhắn mới/liên hệ mới liên
/// quan tới họ — tách khỏi InterestsController vì ListingsController/BuyRequestsController (nơi tạo
/// Interest lúc liên hệ) cũng cần gọi tới cùng logic này.
/// </summary>
public class InterestNotificationService(MarketplaceDbContext db, IHubContext<ChatHub> hubContext)
{
    /// <summary>
    /// Tin nhắn mới nhất (theo Id, tăng dần cùng thời gian) của mỗi Interest — dùng cho cả tính
    /// "chưa đọc" lẫn hiển thị preview ở danh sách "Liên hệ của tôi".
    /// </summary>
    public async Task<Dictionary<int, Message>> GetLastMessagesAsync(IEnumerable<int> interestIds)
    {
        var ids = interestIds.ToList();
        var lastIds = await db.Messages
            .Where(m => ids.Contains(m.InterestId))
            .GroupBy(m => m.InterestId)
            .Select(g => g.Max(m => m.Id))
            .ToListAsync();

        var lastMessages = await db.Messages.Where(m => lastIds.Contains(m.Id)).ToListAsync();
        return lastMessages.ToDictionary(m => m.InterestId, m => m);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        var interests = await db.Interests
            .Where(i => i.InitiatorUserId == userId || i.TargetUserId == userId)
            .ToListAsync();

        var lastMessages = await GetLastMessagesAsync(interests.Select(i => i.Id));
        return interests.Count(i =>
            InterestResponse.ComputeHasUnread(i, userId, lastMessages.GetValueOrDefault(i.Id)?.CreatedAt));
    }

    public async Task PushUnreadCountAsync(int userId)
    {
        var count = await GetUnreadCountAsync(userId);
        await hubContext.Clients.Group(ChatHub.UserGroupName(userId)).SendAsync("UnreadCountChanged", new UnreadCountResponse(count));
    }
}

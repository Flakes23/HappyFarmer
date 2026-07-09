using System.Security.Claims;
using HappyFarmer.MarketplaceService.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketplaceService.Api.Hubs;

/// <summary>
/// Hub chỉ quản lý group membership (join/leave theo Interest) — KHÔNG có RPC gửi tin nhắn.
/// Toàn bộ đường ghi (persist + broadcast) đi qua REST POST ở InterestsController để tránh
/// 2 luồng validate/persist khác nhau, giữ nguyên pattern TanStack Query mutation đang dùng.
/// [Authorize] resolve theo scheme mặc định của service này (TrustedHeaderAuthentication) —
/// đọc thẳng header X-User-Id/X-User-Role do Gateway gắn, không tự verify JWT.
/// </summary>
[Authorize]
public class ChatHub(MarketplaceDbContext db) : Hub
{
    /// <summary>
    /// Tự join group riêng theo user ngay khi kết nối (không cần RPC) — dùng để đẩy sự kiện
    /// "UnreadCountChanged" tới đúng người dùng ở bất kỳ trang nào, không chỉ khi đang mở 1
    /// thread cụ thể (khác với group theo Interest, chỉ join khi mở ChatThread).
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroupName(GetUserId()));
        await base.OnConnectedAsync();
    }

    public static string UserGroupName(int userId) => $"user-{userId}";

    public async Task JoinConversation(int interestId)
    {
        var userId = GetUserId();
        var interest = await db.Interests.AsNoTracking().FirstOrDefaultAsync(i => i.Id == interestId);

        if (interest is null || (interest.InitiatorUserId != userId && interest.TargetUserId != userId))
        {
            throw new HubException("Không có quyền truy cập cuộc trò chuyện này.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(interestId));
    }

    public Task LeaveConversation(int interestId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(interestId));

    public static string GroupName(int interestId) => $"interest-{interestId}";

    private int GetUserId()
    {
        var idClaim = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }
}

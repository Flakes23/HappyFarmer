using HappyFarmer.MarketplaceService.Api.Data;
using HappyFarmer.MarketplaceService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketplaceService.Api.Controllers;

/// <summary>
/// Endpoint nội bộ bảo trì, xác thực bằng API key riêng (không phải JWT người dùng), cùng
/// pattern với Auth Service (xem InternalController.cs của service đó).
/// </summary>
[ApiController]
[Route("api/marketplace/internal")]
[AllowAnonymous]
public class InternalController(
    MarketplaceDbContext db,
    AuthServiceClient authServiceClient,
    DenormalizedUserSyncService userSync,
    IConfiguration configuration) : ControllerBase
{
    private const string ApiKeyHeader = "X-Internal-Api-Key";

    /// <summary>
    /// Đồng bộ lại FarmerName/FarmerAvatarUrl/BuyerName/BuyerAvatarUrl (denormalize từ Auth
    /// Service) cho tin/yêu cầu mua đã tạo trước khi có Kafka auth.user-updated.v1 — chỉ cần
    /// chạy tay 1 lần cho dữ liệu cũ, tin/yêu cầu mua tạo mới sau này đã tự có đúng dữ liệu ngay
    /// từ lúc tạo, và tự cập nhật lại qua Kafka khi user đổi tên/avatar sau đó.
    /// </summary>
    [HttpPost("backfill-avatars")]
    public async Task<IActionResult> BackfillAvatars()
    {
        var expectedKey = configuration["Internal:ApiKey"];
        if (string.IsNullOrEmpty(expectedKey) ||
            !Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey) ||
            providedKey != expectedKey)
        {
            return Unauthorized(new { message = "API key không hợp lệ." });
        }

        var farmerIds = await db.Listings.Select(l => l.FarmerId).Distinct().ToListAsync();
        var listingsUpdated = 0;
        foreach (var farmerId in farmerIds)
        {
            var farmer = await authServiceClient.GetUserAsync(farmerId);
            if (farmer is null) continue;
            var (updated, _) = await userSync.SyncUserAsync(farmerId, farmer.FullName, farmer.AvatarUrl);
            listingsUpdated += updated;
        }

        var buyerIds = await db.BuyRequests.Select(b => b.BuyerId).Distinct().ToListAsync();
        var buyRequestsUpdated = 0;
        foreach (var buyerId in buyerIds)
        {
            var buyer = await authServiceClient.GetUserAsync(buyerId);
            if (buyer is null) continue;
            var (_, updated) = await userSync.SyncUserAsync(buyerId, buyer.FullName, buyer.AvatarUrl);
            buyRequestsUpdated += updated;
        }

        return Ok(new { listingsUpdated, buyRequestsUpdated });
    }
}

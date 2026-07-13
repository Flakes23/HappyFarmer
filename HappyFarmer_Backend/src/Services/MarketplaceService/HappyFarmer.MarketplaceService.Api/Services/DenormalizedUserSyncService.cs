using HappyFarmer.MarketplaceService.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketplaceService.Api.Services;

/// <summary>
/// Cập nhật lại FarmerName/FarmerAvatarUrl (Listing) và BuyerName/BuyerAvatarUrl (BuyRequest) đã
/// denormalize từ Auth Service — dùng chung bởi InternalController.BackfillAvatars (chạy tay 1
/// lần cho dữ liệu cũ) và UserProfileUpdatedConsumer (chạy tự động khi Auth Service publish
/// auth.user-updated.v1).
/// </summary>
public class DenormalizedUserSyncService(MarketplaceDbContext db)
{
    public async Task<(int ListingsUpdated, int BuyRequestsUpdated)> SyncUserAsync(int userId, string fullName, string? avatarUrl)
    {
        var listingsUpdated = await db.Listings.Where(l => l.FarmerId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(l => l.FarmerName, fullName)
                .SetProperty(l => l.FarmerAvatarUrl, avatarUrl));

        var buyRequestsUpdated = await db.BuyRequests.Where(b => b.BuyerId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.BuyerName, fullName)
                .SetProperty(b => b.BuyerAvatarUrl, avatarUrl));

        return (listingsUpdated, buyRequestsUpdated);
    }
}

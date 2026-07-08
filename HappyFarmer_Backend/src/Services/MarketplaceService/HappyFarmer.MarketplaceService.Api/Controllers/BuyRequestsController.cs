using System.Security.Claims;
using HappyFarmer.MarketplaceService.Api.Data;
using HappyFarmer.MarketplaceService.Api.Dtos;
using HappyFarmer.MarketplaceService.Api.Entities;
using HappyFarmer.MarketplaceService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketplaceService.Api.Controllers;

[ApiController]
[Route("api/marketplace/buy-requests")]
public class BuyRequestsController(MarketplaceDbContext db, AuthServiceClient authServiceClient) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<BuyRequestResponse>>> Search(
        [FromQuery] int? productId, [FromQuery] int? regionId, [FromQuery] BuyRequestStatus? status,
        [FromQuery] string? search, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
        [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.BuyRequests
            .Where(br => br.Status == (status ?? BuyRequestStatus.Active))
            .Where(br => productId == null || br.ProductId == productId)
            .Where(br => regionId == null || br.RegionId == regionId)
            .Where(br => minPrice == null || br.MaxPricePerUnit == null || br.MaxPricePerUnit >= minPrice)
            .Where(br => maxPrice == null || br.MaxPricePerUnit == null || br.MaxPricePerUnit <= maxPrice);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(br => br.Description != null && br.Description.Contains(search));
        }

        query = sort switch
        {
            "price_asc" => query.OrderBy(br => br.MaxPricePerUnit),
            "price_desc" => query.OrderByDescending(br => br.MaxPricePerUnit),
            _ => query.OrderByDescending(br => br.CreatedAt),
        };

        var totalCount = await query.CountAsync();
        var buyRequests = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var counts = await GetActiveBuyRequestCountsAsync(buyRequests.Select(br => br.BuyerId));
        var items = buyRequests.Select(br => BuyRequestResponse.FromEntity(br, counts.GetValueOrDefault(br.BuyerId))).ToList();
        return Ok(new PagedResult<BuyRequestResponse>(items, page, pageSize, totalCount));
    }

    [HttpPost]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<BuyRequestResponse>> Create(CreateBuyRequestRequest request)
    {
        var buyerId = GetCurrentUserId()!.Value;
        var buyer = await authServiceClient.GetUserAsync(buyerId);

        var buyRequest = new BuyRequest
        {
            BuyerId = buyerId,
            BuyerName = buyer?.FullName,
            BuyerJoinedAt = buyer?.CreatedAt,
            ProductId = request.ProductId,
            DesiredQuantity = request.DesiredQuantity,
            RegionId = request.RegionId,
            MaxPricePerUnit = request.MaxPricePerUnit,
            Description = request.Description,
        };

        db.BuyRequests.Add(buyRequest);
        await db.SaveChangesAsync();

        var count = await db.BuyRequests.CountAsync(br => br.BuyerId == buyerId && br.Status == BuyRequestStatus.Active);
        return Ok(BuyRequestResponse.FromEntity(buyRequest, count));
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    private async Task<Dictionary<int, int>> GetActiveBuyRequestCountsAsync(IEnumerable<int> buyerIds)
    {
        var ids = buyerIds.Distinct().ToList();
        return await db.BuyRequests
            .Where(br => ids.Contains(br.BuyerId) && br.Status == BuyRequestStatus.Active)
            .GroupBy(br => br.BuyerId)
            .Select(g => new { BuyerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BuyerId, x => x.Count);
    }
}

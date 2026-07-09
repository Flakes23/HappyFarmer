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
public class BuyRequestsController(MarketplaceDbContext db, AuthServiceClient authServiceClient, InterestNotificationService notifier) : ControllerBase
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

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<BuyRequestResponse>> GetById(int id)
    {
        var buyRequest = await db.BuyRequests.SingleOrDefaultAsync(br => br.Id == id);
        if (buyRequest is null) return NotFound();

        var count = await db.BuyRequests.CountAsync(br => br.BuyerId == buyRequest.BuyerId && br.Status == BuyRequestStatus.Active);
        return Ok(BuyRequestResponse.FromEntity(buyRequest, count));
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
            Unit = request.Unit,
            RegionId = request.RegionId,
            MaxPricePerUnit = request.MaxPricePerUnit,
            Description = request.Description,
        };

        db.BuyRequests.Add(buyRequest);
        await db.SaveChangesAsync();

        var count = await db.BuyRequests.CountAsync(br => br.BuyerId == buyerId && br.Status == BuyRequestStatus.Active);
        return Ok(BuyRequestResponse.FromEntity(buyRequest, count));
    }

    [HttpPost("{id:int}/contact")]
    [Authorize(Roles = "Farmer")]
    public async Task<ActionResult<InterestResponse>> Contact(int id, ContactListingRequest request)
    {
        var buyRequest = await db.BuyRequests.FindAsync(id);
        if (buyRequest is null) return NotFound();

        var interest = new Interest
        {
            BuyRequestId = buyRequest.Id,
            InitiatorUserId = GetCurrentUserId()!.Value,
            TargetUserId = buyRequest.BuyerId,
            Message = request.Message,
            InitiatorReadAt = DateTime.UtcNow,
        };

        db.Interests.Add(interest);
        await db.SaveChangesAsync();
        await notifier.PushUnreadCountAsync(interest.TargetUserId);

        // TODO (Phase 4): publish "marketplace.new-interest.v1" lên Kafka để Notification Service
        // consume — chưa wiring vì Kafka chưa setup ở local (xem docs/architecture/04-roadmap.md).

        return Ok(InterestResponse.FromEntity(interest, interest.InitiatorUserId, null));
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

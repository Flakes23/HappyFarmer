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
[Route("api/marketplace/listings")]
public class ListingsController(MarketplaceDbContext db, AuthServiceClient authServiceClient, InterestNotificationService notifier) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ListingResponse>>> Search(
        [FromQuery] int? productId, [FromQuery] int? regionId, [FromQuery] ListingStatus? status,
        [FromQuery] string? search, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
        [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Listings
            .Include(l => l.Images)
            .Where(l => l.Status == (status ?? ListingStatus.Active))
            .Where(l => productId == null || l.ProductId == productId)
            .Where(l => regionId == null || l.RegionId == regionId)
            .Where(l => minPrice == null || l.PricePerUnit >= minPrice)
            .Where(l => maxPrice == null || l.PricePerUnit <= maxPrice);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(l =>
                (l.Description != null && l.Description.Contains(search)) || l.Unit.Contains(search));
        }

        query = sort switch
        {
            "price_asc" => query.OrderBy(l => l.PricePerUnit),
            "price_desc" => query.OrderByDescending(l => l.PricePerUnit),
            _ => query.OrderByDescending(l => l.CreatedAt),
        };

        var totalCount = await query.CountAsync();
        var listings = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var counts = await GetActiveListingCountsAsync(listings.Select(l => l.FarmerId));
        var items = listings.Select(l => ListingResponse.FromEntity(l, counts.GetValueOrDefault(l.FarmerId))).ToList();
        return Ok(new PagedResult<ListingResponse>(items, page, pageSize, totalCount));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ListingResponse>> GetById(int id)
    {
        var listing = await db.Listings.Include(l => l.Images).SingleOrDefaultAsync(l => l.Id == id);
        if (listing is null) return NotFound();

        var count = await db.Listings.CountAsync(l => l.FarmerId == listing.FarmerId && l.Status == ListingStatus.Active);
        return Ok(ListingResponse.FromEntity(listing, count));
    }

    [HttpGet("/api/marketplace/my-listings")]
    [Authorize]
    public async Task<ActionResult<List<ListingResponse>>> GetMyListings()
    {
        var farmerId = GetCurrentUserId();
        var listings = await db.Listings
            .Include(l => l.Images)
            .Where(l => l.FarmerId == farmerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var counts = await GetActiveListingCountsAsync(listings.Select(l => l.FarmerId));
        return Ok(listings.Select(l => ListingResponse.FromEntity(l, counts.GetValueOrDefault(l.FarmerId))));
    }

    [HttpPost]
    [Authorize(Roles = "Farmer")]
    public async Task<ActionResult<ListingResponse>> Create(CreateListingRequest request)
    {
        var farmerId = GetCurrentUserId()!.Value;
        var farmer = await authServiceClient.GetUserAsync(farmerId);

        var listing = new Listing
        {
            FarmerId = farmerId,
            FarmerName = farmer?.FullName,
            FarmerJoinedAt = farmer?.CreatedAt,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Unit = request.Unit,
            PricePerUnit = request.PricePerUnit,
            RegionId = request.RegionId,
            Description = request.Description,
            ExpiresAt = request.ExpiresAt,
        };

        if (request.ImageUrls is { Count: > 0 })
        {
            listing.Images = request.ImageUrls
                .Select((url, index) => new ListingImage { ImageUrl = url, SortOrder = index })
                .ToList();
        }

        db.Listings.Add(listing);
        await db.SaveChangesAsync();

        var count = await db.Listings.CountAsync(l => l.FarmerId == farmerId && l.Status == ListingStatus.Active);
        return CreatedAtAction(nameof(GetById), new { id = listing.Id }, ListingResponse.FromEntity(listing, count));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Farmer")]
    public async Task<ActionResult<ListingResponse>> Update(int id, UpdateListingRequest request)
    {
        var listing = await db.Listings.Include(l => l.Images).SingleOrDefaultAsync(l => l.Id == id);
        if (listing is null) return NotFound();
        if (listing.FarmerId != GetCurrentUserId()) return Forbid();

        if (request.Quantity is not null) listing.Quantity = request.Quantity.Value;
        if (request.PricePerUnit is not null) listing.PricePerUnit = request.PricePerUnit.Value;
        if (request.Description is not null) listing.Description = request.Description;
        if (request.ExpiresAt is not null) listing.ExpiresAt = request.ExpiresAt;

        await db.SaveChangesAsync();

        var count = await db.Listings.CountAsync(l => l.FarmerId == listing.FarmerId && l.Status == ListingStatus.Active);
        return Ok(ListingResponse.FromEntity(listing, count));
    }

    [HttpPatch("{id:int}/close")]
    [Authorize(Roles = "Farmer")]
    public async Task<ActionResult<ListingResponse>> Close(int id)
    {
        var listing = await db.Listings.Include(l => l.Images).SingleOrDefaultAsync(l => l.Id == id);
        if (listing is null) return NotFound();
        if (listing.FarmerId != GetCurrentUserId()) return Forbid();

        listing.Status = ListingStatus.Closed;
        await db.SaveChangesAsync();

        var count = await db.Listings.CountAsync(l => l.FarmerId == listing.FarmerId && l.Status == ListingStatus.Active);
        return Ok(ListingResponse.FromEntity(listing, count));
    }

    [HttpPost("{id:int}/contact")]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<InterestResponse>> Contact(int id, ContactListingRequest request)
    {
        var listing = await db.Listings.FindAsync(id);
        if (listing is null) return NotFound();

        var interest = new Interest
        {
            ListingId = listing.Id,
            InitiatorUserId = GetCurrentUserId()!.Value,
            TargetUserId = listing.FarmerId,
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

    private async Task<Dictionary<int, int>> GetActiveListingCountsAsync(IEnumerable<int> farmerIds)
    {
        var ids = farmerIds.Distinct().ToList();
        return await db.Listings
            .Where(l => ids.Contains(l.FarmerId) && l.Status == ListingStatus.Active)
            .GroupBy(l => l.FarmerId)
            .Select(g => new { FarmerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.FarmerId, x => x.Count);
    }
}

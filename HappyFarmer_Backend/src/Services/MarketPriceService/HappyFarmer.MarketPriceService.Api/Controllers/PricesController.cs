using System.Security.Claims;
using HappyFarmer.MarketPriceService.Api.Data;
using HappyFarmer.MarketPriceService.Api.Dtos;
using HappyFarmer.MarketPriceService.Api.Entities;
using HappyFarmer.MarketPriceService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketPriceService.Api.Controllers;

[ApiController]
[Route("api/market-price/prices")]
public class PricesController(MarketPriceDbContext db, PriceCacheService cache) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<PriceResponse>>> GetCurrentPrices(
        [FromQuery] int? productId, [FromQuery] int? regionId, [FromQuery] DateOnly? date,
        [FromQuery] string? search, [FromQuery] int? categoryId, [FromQuery] int? subCategoryId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var asOfDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var hasExtraFilters = !string.IsNullOrWhiteSpace(search) || categoryId is not null || subCategoryId is not null;

        if (productId is not null && regionId is not null && !hasExtraFilters && page == 1)
        {
            var cached = await cache.GetCurrentPriceAsync(productId.Value, regionId.Value);
            if (cached is not null)
            {
                return Ok(new PagedResult<PriceResponse>([cached], 1, pageSize, 1));
            }
        }

        var candidates = await db.PriceEntries
            .Include(pe => pe.Product)
            .Include(pe => pe.Region)
            .Where(pe => pe.Status == PriceEntryStatus.Approved && pe.EffectiveDate <= asOfDate)
            .Where(pe => productId == null || pe.ProductId == productId)
            .Where(pe => regionId == null || pe.RegionId == regionId)
            .Where(pe => string.IsNullOrWhiteSpace(search) || pe.Product.NameVi.Contains(search))
            .Where(pe => categoryId == null || pe.Product.SubCategory.CategoryId == categoryId)
            .Where(pe => subCategoryId == null || pe.Product.SubCategoryId == subCategoryId)
            .OrderByDescending(pe => pe.EffectiveDate)
            .ThenByDescending(pe => pe.CreatedAt)
            .ToListAsync();

        var latestPerGroup = candidates
            .GroupBy(pe => (pe.ProductId, pe.RegionId, pe.Unit))
            .Select(g => g.First())
            .OrderBy(pe => pe.Product.NameVi)
            .ToList();

        var totalCount = latestPerGroup.Count;
        var paged = latestPerGroup.Skip((page - 1) * pageSize).Take(pageSize).Select(ToResponse).ToList();

        if (productId is not null && regionId is not null && !hasExtraFilters && page == 1 && totalCount == 1)
        {
            await cache.SetCurrentPriceAsync(productId.Value, regionId.Value, paged[0]);
        }

        return Ok(new PagedResult<PriceResponse>(paged, page, pageSize, totalCount));
    }

    [HttpGet("{productId:int}/history")]
    [AllowAnonymous]
    public async Task<ActionResult<List<PriceHistoryPoint>>> GetHistory(
        int productId, [FromQuery] int? regionId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var fromDate = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));
        var toDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var history = await db.PriceEntries
            .Where(pe => pe.Status == PriceEntryStatus.Approved && pe.ProductId == productId)
            .Where(pe => regionId == null || pe.RegionId == regionId)
            .Where(pe => pe.EffectiveDate >= fromDate && pe.EffectiveDate <= toDate)
            .OrderBy(pe => pe.EffectiveDate)
            .Select(pe => new PriceHistoryPoint(pe.EffectiveDate, pe.Price, pe.Unit))
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<ActionResult<List<TrendingItem>>> GetTrending()
    {
        var cached = await cache.GetTrendingAsync();
        if (cached is not null)
        {
            return Ok(cached);
        }

        var recent = await db.PriceEntries
            .Include(pe => pe.Product)
            .Include(pe => pe.Region)
            .Where(pe => pe.Status == PriceEntryStatus.Approved)
            .OrderByDescending(pe => pe.EffectiveDate)
            .ThenByDescending(pe => pe.CreatedAt)
            .Take(2000)
            .ToListAsync();

        var trending = recent
            .GroupBy(pe => (pe.ProductId, pe.RegionId, pe.Unit))
            .Select(g =>
            {
                var ordered = g.OrderByDescending(pe => pe.EffectiveDate).ThenByDescending(pe => pe.CreatedAt).ToList();
                var current = ordered[0];
                var previous = ordered.Count > 1 ? ordered[1] : null;
                decimal? changePercent = previous is { Price: > 0 }
                    ? Math.Round((current.Price - previous.Price) / previous.Price * 100, 2)
                    : null;
                return new TrendingItem(
                    current.ProductId, current.Product.NameVi, current.RegionId, current.Region.MarketName,
                    current.Price, previous?.Price, changePercent, current.Unit);
            })
            .Where(t => t.ChangePercent is not null)
            .OrderByDescending(t => Math.Abs(t.ChangePercent!.Value))
            .Take(10)
            .ToList();

        await cache.SetTrendingAsync(trending);
        return Ok(trending);
    }

    [Authorize(Roles = "Farmer")]
    [HttpPost]
    public async Task<ActionResult<PriceEntryResponse>> SubmitPrice(SubmitPriceRequest request)
    {
        var productExists = await db.Products.AnyAsync(p => p.Id == request.ProductId);
        var regionExists = await db.Regions.AnyAsync(r => r.Id == request.RegionId);
        if (!productExists || !regionExists)
        {
            return BadRequest(new { message = "Nông sản hoặc khu vực không tồn tại." });
        }

        var entry = new PriceEntry
        {
            ProductId = request.ProductId,
            RegionId = request.RegionId,
            Price = request.Price,
            Source = PriceSource.Community,
            SubmittedByUserId = GetCurrentUserId(),
            Status = PriceEntryStatus.Pending,
            EffectiveDate = request.EffectiveDate,
        };

        db.PriceEntries.Add(entry);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(SubmitPrice), new { id = entry.Id }, PriceEntryResponse.FromEntity(entry));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<PriceEntryResponse>> ApprovePrice(int id)
    {
        var entry = await db.PriceEntries.FindAsync(id);
        if (entry is null) return NotFound();

        entry.Status = PriceEntryStatus.Approved;
        await db.SaveChangesAsync();
        await cache.InvalidateCurrentPriceAsync(entry.ProductId, entry.RegionId);

        return Ok(PriceEntryResponse.FromEntity(entry));
    }

    private static PriceResponse ToResponse(PriceEntry pe) => new(
        pe.ProductId, pe.Product.NameVi, pe.RegionId, pe.Region.MarketName, pe.Price, pe.Source.ToString(), pe.EffectiveDate, pe.Unit);

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(idClaim, out var id) ? id : null;
    }
}

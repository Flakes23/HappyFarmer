using HappyFarmer.MarketPriceService.Api.Data;
using HappyFarmer.MarketPriceService.Api.Dtos;
using HappyFarmer.MarketPriceService.Api.Entities;
using HappyFarmer.MarketPriceService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketPriceService.Api.Controllers;

/// <summary>
/// Endpoint nội bộ cho crawler/cronjob nạp giá — xác thực bằng API key riêng
/// (không phải JWT người dùng), theo docs/architecture/services/market-price-service.md.
/// </summary>
[ApiController]
[Route("api/market-price/internal")]
[AllowAnonymous]
public class InternalController(MarketPriceDbContext db, PriceCacheService cache, IConfiguration configuration) : ControllerBase
{
    private const string ApiKeyHeader = "X-Internal-Api-Key";

    [HttpPost("crawl-ingest")]
    public async Task<ActionResult<PriceEntryResponse>> CrawlIngest(CrawlIngestRequest request)
    {
        var expectedKey = configuration["Internal:ApiKey"];
        if (string.IsNullOrEmpty(expectedKey) ||
            !Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey) ||
            providedKey != expectedKey)
        {
            return Unauthorized(new { message = "API key không hợp lệ." });
        }

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
            Source = PriceSource.Crawled,
            Status = PriceEntryStatus.Approved,
            EffectiveDate = request.EffectiveDate,
        };

        db.PriceEntries.Add(entry);
        await db.SaveChangesAsync();
        await cache.InvalidateCurrentPriceAsync(entry.ProductId, entry.RegionId);

        return Ok(PriceEntryResponse.FromEntity(entry));
    }
}

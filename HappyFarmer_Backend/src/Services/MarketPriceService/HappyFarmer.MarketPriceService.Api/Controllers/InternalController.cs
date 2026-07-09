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

    /// <summary>
    /// Find-or-create toàn bộ chuỗi Category -&gt; SubCategory -&gt; Product -&gt; Region theo tên trước
    /// khi ghi PriceEntry — crawler không cần biết trước Id, chỉ cần gửi đúng tên lấy được từ
    /// nguồn. Product đã tồn tại thì cập nhật lại SubCategory/Unit/ImageUrl theo lần crawl mới
    /// nhất (site có thể đổi cách phân loại/đơn vị theo thời gian).
    /// </summary>
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

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == request.CategoryName);
        if (category is null)
        {
            category = new Category { Name = request.CategoryName };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
        }

        var subCategory = await db.SubCategories.FirstOrDefaultAsync(sc => sc.CategoryId == category.Id && sc.Name == request.SubCategoryName);
        if (subCategory is null)
        {
            subCategory = new SubCategory { CategoryId = category.Id, Name = request.SubCategoryName };
            db.SubCategories.Add(subCategory);
            await db.SaveChangesAsync();
        }

        var product = await db.Products.FirstOrDefaultAsync(p => p.NameVi == request.ProductName);
        if (product is null)
        {
            product = new Product { NameVi = request.ProductName, SubCategoryId = subCategory.Id, Unit = request.ProductUnit, ImageUrl = request.ImageUrl };
            db.Products.Add(product);
        }
        else
        {
            product.SubCategoryId = subCategory.Id;
            product.Unit = request.ProductUnit;
            product.ImageUrl = request.ImageUrl ?? product.ImageUrl;
        }

        await db.SaveChangesAsync();

        var region = await db.Regions.FirstOrDefaultAsync(r => r.ProvinceName == request.RegionProvinceName && r.MarketName == request.RegionMarketName);
        if (region is null)
        {
            region = new Region { ProvinceName = request.RegionProvinceName, MarketName = request.RegionMarketName };
            db.Regions.Add(region);
            await db.SaveChangesAsync();
        }

        var entry = new PriceEntry
        {
            ProductId = product.Id,
            RegionId = region.Id,
            Price = request.Price,
            Unit = request.Unit,
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

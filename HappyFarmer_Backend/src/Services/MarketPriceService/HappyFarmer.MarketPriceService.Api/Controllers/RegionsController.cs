using HappyFarmer.MarketPriceService.Api.Data;
using HappyFarmer.MarketPriceService.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketPriceService.Api.Controllers;

[ApiController]
[Route("api/market-price/regions")]
[AllowAnonymous]
public class RegionsController(MarketPriceDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RegionResponse>>> GetRegions()
    {
        var regions = await db.Regions.OrderBy(r => r.ProvinceName).ToListAsync();
        return Ok(regions.Select(RegionResponse.FromEntity));
    }

    /// <summary>
    /// Tra tên hiển thị cho đúng vài Id cụ thể (vd. chatbot AI Advisory resolve tên khu vực cho
    /// listing/giá vừa tìm được) — tránh phải tải toàn bộ bảng Region rồi tự lọc.
    /// </summary>
    [HttpGet("by-ids")]
    public async Task<ActionResult<List<RegionResponse>>> GetRegionsByIds([FromQuery] List<int> ids)
    {
        if (ids is not { Count: > 0 })
        {
            return Ok(new List<RegionResponse>());
        }

        var regions = await db.Regions.Where(r => ids.Contains(r.Id)).ToListAsync();
        return Ok(regions.Select(RegionResponse.FromEntity));
    }
}

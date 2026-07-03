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
}

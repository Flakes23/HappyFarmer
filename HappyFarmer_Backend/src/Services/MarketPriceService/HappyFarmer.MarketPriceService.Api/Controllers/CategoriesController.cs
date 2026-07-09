using HappyFarmer.MarketPriceService.Api.Data;
using HappyFarmer.MarketPriceService.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketPriceService.Api.Controllers;

[ApiController]
[Route("api/market-price/categories")]
[AllowAnonymous]
public class CategoriesController(MarketPriceDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryResponse>>> GetCategories()
    {
        var categories = await db.Categories.OrderBy(c => c.Name).ToListAsync();
        return Ok(categories.Select(CategoryResponse.FromEntity));
    }

    [HttpGet("{categoryId:int}/sub-categories")]
    public async Task<ActionResult<List<SubCategoryResponse>>> GetSubCategories(int categoryId)
    {
        var subCategories = await db.SubCategories
            .Include(sc => sc.Category)
            .Where(sc => sc.CategoryId == categoryId)
            .OrderBy(sc => sc.Name)
            .ToListAsync();
        return Ok(subCategories.Select(SubCategoryResponse.FromEntity));
    }
}

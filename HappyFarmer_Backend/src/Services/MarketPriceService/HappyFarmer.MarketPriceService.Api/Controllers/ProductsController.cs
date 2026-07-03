using HappyFarmer.MarketPriceService.Api.Data;
using HappyFarmer.MarketPriceService.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketPriceService.Api.Controllers;

[ApiController]
[Route("api/market-price/products")]
[AllowAnonymous]
public class ProductsController(MarketPriceDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetProducts()
    {
        var products = await db.Products.OrderBy(p => p.NameVi).ToListAsync();
        return Ok(products.Select(ProductResponse.FromEntity));
    }
}

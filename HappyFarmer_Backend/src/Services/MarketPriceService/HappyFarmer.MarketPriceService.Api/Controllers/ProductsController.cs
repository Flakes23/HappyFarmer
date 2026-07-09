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
    public async Task<ActionResult<List<ProductResponse>>> GetProducts([FromQuery] string? search, [FromQuery] int limit = 20)
    {
        var query = db.Products
            .Include(p => p.SubCategory).ThenInclude(sc => sc.Category)
            .Where(p => string.IsNullOrWhiteSpace(search) || p.NameVi.Contains(search))
            .OrderBy(p => p.NameVi);

        // Không giới hạn khi không tìm kiếm — các trang khác (ListingCard, BuyRequestCard...) cần
        // tải toàn bộ danh sách để tra tên theo Id. Chỉ giới hạn khi có search (autocomplete),
        // tránh trả về toàn bộ bảng nếu sau này số lượng sản phẩm lớn.
        var products = string.IsNullOrWhiteSpace(search)
            ? await query.ToListAsync()
            : await query.Take(Math.Clamp(limit, 1, 100)).ToListAsync();

        return Ok(products.Select(ProductResponse.FromEntity));
    }
}

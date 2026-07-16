using HappyFarmer.AuthService.Api.Data;
using HappyFarmer.AuthService.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.AuthService.Api.Controllers;

[ApiController]
[Route("api/auth/provinces")]
[AllowAnonymous]
public class ProvincesController(AuthDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProvinceResponse>>> GetProvinces()
    {
        var provinces = await db.Provinces.OrderBy(p => p.Id).ToListAsync();
        return Ok(provinces.Select(ProvinceResponse.FromEntity));
    }
}

using HappyFarmer.AuthService.Api.Data;
using HappyFarmer.AuthService.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.AuthService.Api.Controllers;

/// <summary>
/// Endpoint nội bộ cho các service khác tra tên/ngày tham gia người dùng (vd. Marketplace Service
/// hiển thị yếu tố tin cậy) — xác thực bằng API key riêng (không phải JWT người dùng), cùng pattern
/// với Market Price Service (xem InternalController.cs của service đó).
/// </summary>
[ApiController]
[Route("api/auth/internal")]
[AllowAnonymous]
public class InternalController(AuthDbContext db, IConfiguration configuration) : ControllerBase
{
    private const string ApiKeyHeader = "X-Internal-Api-Key";

    [HttpGet("users/lookup")]
    public async Task<ActionResult<List<UserLookupResponse>>> LookupUsers([FromQuery] List<int> ids)
    {
        var expectedKey = configuration["Internal:ApiKey"];
        if (string.IsNullOrEmpty(expectedKey) ||
            !Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey) ||
            providedKey != expectedKey)
        {
            return Unauthorized(new { message = "API key không hợp lệ." });
        }

        if (ids is not { Count: > 0 })
        {
            return Ok(new List<UserLookupResponse>());
        }

        var users = await db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
        return Ok(users.Select(UserLookupResponse.FromEntity));
    }
}

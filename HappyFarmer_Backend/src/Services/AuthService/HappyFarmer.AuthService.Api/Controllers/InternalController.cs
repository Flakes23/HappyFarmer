using HappyFarmer.AuthService.Api.Data;
using HappyFarmer.AuthService.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.AuthService.Api.Controllers;

/// <summary>
/// Endpoint nội bộ cho các service khác tra tên/ngày tham gia người dùng (vd. Marketplace Service
/// hiển thị yếu tố tin cậy, AI Advisory Service cho chatbot xưng hô cá nhân hóa) — xác thực bằng
/// API key riêng (không phải JWT người dùng), cùng pattern với Market Price Service.
///
/// Mỗi service gọi vào đây dùng 1 key RIÊNG (cấu hình <c>Internal:ApiKeys:&lt;TênService&gt;</c>,
/// vd. <c>Internal:ApiKeys:Marketplace</c>, <c>Internal:ApiKeys:AiAdvisory</c>) thay vì 1 key dùng
/// chung — key của service nào lộ/cần thu hồi thì đổi đúng entry đó, không ảnh hưởng service khác.
/// Thêm caller mới chỉ cần thêm 1 dòng config, không cần sửa code (so sánh theo value, không cần
/// biết trước tên caller).
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
        if (!IsValidInternalKey())
        {
            return Unauthorized(new { message = "API key không hợp lệ." });
        }

        if (ids is not { Count: > 0 })
        {
            return Ok(new List<UserLookupResponse>());
        }

        var users = await db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
        var provinceNames = await db.Provinces
            .Where(p => users.Select(u => u.ProvinceId).Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        return Ok(users.Select(u => UserLookupResponse.FromEntity(
            u, u.ProvinceId is int pid ? provinceNames.GetValueOrDefault(pid) : null)));
    }

    private bool IsValidInternalKey()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey) || string.IsNullOrEmpty(providedKey))
        {
            return false;
        }

        return configuration.GetSection("Internal:ApiKeys").GetChildren().Any(k => k.Value == providedKey);
    }
}

using System.Security.Claims;
using HappyFarmer.AuthService.Api.Data;
using HappyFarmer.AuthService.Api.Dtos;
using HappyFarmer.AuthService.Api.Entities;
using HappyFarmer.AuthService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AuthDbContext db,
    JwtTokenService tokenService,
    LoginRateLimiter rateLimiter) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (request.Role == UserRole.Admin)
        {
            return BadRequest(new { message = "Không thể tự đăng ký với vai trò Admin." });
        }

        var phoneExists = await db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (phoneExists)
        {
            return Conflict(new { message = "Số điện thoại đã được sử dụng." });
        }

        if (!string.IsNullOrEmpty(request.Email) && await db.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Conflict(new { message = "Email đã được sử dụng." });
        }

        var user = new User
        {
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = request.Role,
            ProvinceId = request.ProvinceId,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (response, _) = await IssueTokensAsync(user);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var rateLimitKey = request.PhoneNumber;
        if (await rateLimiter.IsBlockedAsync(rateLimitKey))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests,
                new { message = "Đăng nhập sai quá nhiều lần, vui lòng thử lại sau." });
        }

        var user = await db.Users.SingleOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await rateLimiter.RegisterFailedAttemptAsync(rateLimitKey);
            return Unauthorized(new { message = "Số điện thoại hoặc mật khẩu không đúng." });
        }

        await rateLimiter.ResetAsync(rateLimitKey);
        var (response, _) = await IssueTokensAsync(user);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken(RefreshTokenRequest request)
    {
        var hash = JwtTokenService.HashRefreshToken(request.RefreshToken);
        var existing = await db.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.TokenHash == hash);

        if (existing is null || !existing.IsActive)
        {
            return Unauthorized(new { message = "Refresh token không hợp lệ hoặc đã hết hạn." });
        }

        var (response, newRefreshToken) = await IssueTokensAsync(existing.User);

        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenId = newRefreshToken.Id;
        await db.SaveChangesAsync();

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        var hash = JwtTokenService.HashRefreshToken(request.RefreshToken);
        var existing = await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.TokenHash == hash);
        if (existing is not null && existing.RevokedAt is null)
        {
            existing.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        var user = await GetCurrentUserAsync();
        return user is null ? NotFound() : Ok(UserResponse.FromEntity(user));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserResponse>> UpdateMe(UpdateProfileRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName;
        if (request.Email is not null) user.Email = request.Email;
        if (request.ProvinceId is not null) user.ProvinceId = request.ProvinceId;
        if (request.AvatarUrl is not null) user.AvatarUrl = request.AvatarUrl;

        await db.SaveChangesAsync();
        return Ok(UserResponse.FromEntity(user));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Mật khẩu hiện tại không đúng." });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet("users")]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        var users = await db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return Ok(users.Select(UserResponse.FromEntity));
    }

    private async Task<(AuthResponse Response, RefreshToken RefreshToken)> IssueTokensAsync(User user)
    {
        var accessToken = tokenService.GenerateAccessToken(user);
        var (refreshTokenPlainText, refreshTokenHash, expiresAt) = tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = expiresAt,
        };
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        var response = new AuthResponse(
            accessToken,
            refreshTokenPlainText,
            DateTime.UtcNow.AddMinutes(tokenService.AccessTokenMinutes),
            UserResponse.FromEntity(user));

        return (response, refreshToken);
    }

    private Task<User?> GetCurrentUserAsync()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(idClaim, out var id)
            ? db.Users.SingleOrDefaultAsync(u => u.Id == id)
            : Task.FromResult<User?>(null);
    }
}
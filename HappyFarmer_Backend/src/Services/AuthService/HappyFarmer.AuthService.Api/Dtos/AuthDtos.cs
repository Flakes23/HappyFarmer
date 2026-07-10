using System.ComponentModel.DataAnnotations;
using HappyFarmer.AuthService.Api.Entities;

namespace HappyFarmer.AuthService.Api.Dtos;

public record RegisterRequest(
    [Required] string PhoneNumber,
    string? Email,
    [Required, MinLength(6)] string Password,
    [Required] string FullName,
    [Required] UserRole Role,
    int? ProvinceId);

public record LoginRequest([Required] string PhoneNumber, [Required] string Password);

public record RefreshTokenRequest([Required] string RefreshToken);

public record UpdateProfileRequest(string? FullName, string? Email, int? ProvinceId, string? AvatarUrl);

public record ChangePasswordRequest([Required] string CurrentPassword, [Required, MinLength(6)] string NewPassword);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, UserResponse User);

public record UserResponse(int Id, string? PhoneNumber, string? Email, string FullName, string Role, int? ProvinceId, bool IsActive, DateTime CreatedAt, string? AvatarUrl)
{
    public static UserResponse FromEntity(User user) => new(
        user.Id, user.PhoneNumber, user.Email, user.FullName, user.Role.ToString(), user.ProvinceId, user.IsActive, user.CreatedAt, user.AvatarUrl);
}

public record UserLookupResponse(int Id, string FullName, DateTime CreatedAt, string? AvatarUrl)
{
    public static UserLookupResponse FromEntity(User user) => new(user.Id, user.FullName, user.CreatedAt, user.AvatarUrl);
}
using System.Security.Claims;
using ChannelCenter.API.DTOs.Auth;

namespace ChannelCenter.API.Services.Auth;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage, AuthResponseDto? Data)> RegisterPatientAsync(PatientRegisterDto dto);
    Task<(bool Success, string? ErrorMessage, AuthResponseDto? Data)> RegisterAdminAsync(AdminRegisterDto dto);
    Task<(bool Success, string? ErrorMessage, AuthResponseDto? Data)> LoginAsync(PatientLoginDto dto);
    Task<(bool Success, string? ErrorMessage, AuthResponseDto? Data)> LoginAdminAsync(AdminLoginDto dto);
    Task<(bool Success, string? ErrorMessage, AuthUserDto? Data)> GetCurrentUserProfileAsync(ClaimsPrincipal user);
}

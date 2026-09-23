using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Auth;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.Auth;

namespace ChannelCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _dbContext;
    private readonly IAuthService _authService;

    public AuthController(IConfiguration config, ApplicationDbContext dbContext, IAuthService authService)
    {
        _config = config;
        _dbContext = dbContext;
        _authService = authService;
    }

    // POST: api/auth/register
    // Patient onboarding and account registration
    [HttpPost("register")]
    public async Task<IActionResult> RegisterPatient([FromBody] PatientRegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage, data) = await _authService.RegisterPatientAsync(dto);
        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(data);
    }

    // POST: api/auth/login
    // Patient and user login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] PatientLoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage, data) = await _authService.LoginAsync(dto);
        if (!success)
        {
            return Unauthorized(new { message = errorMessage });
        }

        return Ok(data);
    }

    // GET: api/auth/me
    // Retrieve current authenticated user and linked patient profile
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var (success, errorMessage, data) = await _authService.GetCurrentUserProfileAsync(User);
        if (!success)
        {
            return NotFound(new { message = errorMessage });
        }

        return Ok(data);
    }

    // POST: api/auth/dev-login
    // Body example: 1 (Retained for admin development & testing)
    [HttpPost("dev-login")]
    public async Task<IActionResult> GenerateDevToken([FromBody] int adminUserId)
    {
        if (adminUserId <= 0)
        {
            return BadRequest(new { message = "A valid adminUserId is required." });
        }

        var adminUser = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == adminUserId);
        if (adminUser == null)
        {
            return NotFound(new { message = $"Admin user with ID {adminUserId} was not found." });
        }

        if (adminUser.Role != UserRole.Admin)
        {
            return BadRequest(new { message = $"User {adminUserId} is not an admin account." });
        }

        var jwtKey = _config["Jwt:SecretKey"] ?? "ChannelCenterDevSecret_MustBe32CharsOrMore!";
        var jwtIssuer = _config["Jwt:Issuer"] ?? "ChannelCenterAPI";
        var jwtAudience = _config["Jwt:Audience"] ?? "ChannelCenterClients";
        var expiryMinutes = _config.GetValue<int?>("Jwt:ExpiryMinutes") ?? 60;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
            new Claim(ClaimTypes.Name, adminUser.FullName),
            new Claim(ClaimTypes.Email, adminUser.Email),
            new Claim(ClaimTypes.Role, adminUser.Role.ToString())
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new
        {
            token = tokenHandler.WriteToken(token),
            tokenType = "Bearer",
            expiresAt,
            adminUser = new
            {
                adminUser.Id,
                adminUser.FullName,
                adminUser.Email,
                Role = adminUser.Role.ToString()
            }
        });
    }
}
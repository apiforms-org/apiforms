using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace APIForms.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IUserRepository users, IConfiguration config) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest(new { message = "Email y password son obligatorios." });
        }

        var email = req.Email.Trim().ToLowerInvariant();
        var exists = await users.GetByEmailAsync(email, ct);
        if (exists is not null)
        {
            return Conflict(new { message = "El usuario ya existe." });
        }

        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var hash = HashPassword(req.Password, salt);

        var user = await users.CreateAsync(new ApiFormUser
        {
            Email = email,
            PasswordSalt = salt,
            PasswordHash = hash,
            TenantId = Guid.NewGuid().ToString("N")
        }, ct);

        var token = BuildToken(user.TenantId, user.Id, "user");
        return Ok(new AuthResponse(token, user.TenantId, user.Id, user.Email));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest(new { message = "Email y password son obligatorios." });
        }

        var email = req.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(email, ct);
        if (user is null)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var hash = HashPassword(req.Password, user.PasswordSalt);
        if (!CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(hash), Convert.FromBase64String(user.PasswordHash)))
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var token = BuildToken(user.TenantId, user.Id, "user");
        return Ok(new AuthResponse(token, user.TenantId, user.Id, user.Email));
    }

    private string BuildToken(string tenantId, string userId, string userType)
    {
        var issuer = config["Jwt:Issuer"] ?? "apiforms";
        var audience = config["Jwt:Audience"] ?? "apiforms-clients";
        var key = config["Jwt:Key"] ?? "ChangeThisJwtKeyForProduction_AtLeast32Chars";

        var claims = new[]
        {
            new Claim("tenantId", tenantId),
            new Claim("userId", userId),
            new Claim("userType", userType)
        };

        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password, string salt)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{password}"));
        return Convert.ToBase64String(bytes);
    }
}

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string Token, string TenantId, string UserId, string Email);

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PosServer.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace PosServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly CentralDbContext _dbContext;

    public AuthController(IConfiguration configuration, CentralDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

        [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try 
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());
            
            if (user != null && user.Pin == request.Password)
            {
                var token = GenerateJwtToken(user.Username, user.TenantId);
                return Ok(new { Token = token, TenantId = user.TenantId ?? "default" });
            }
            return Unauthorized();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = ex.Message, Stack = ex.StackTrace, Inner = ex.InnerException?.Message });
        }
    }

    private string GenerateJwtToken(string username, string tenantId)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username ?? "unknown"),
            new Claim("TenantId", tenantId ?? "default"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

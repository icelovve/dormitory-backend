using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Context;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly string jwtSecretKey = "my_super_secret_key_1234567890_!@#$%^&*";

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var key = Encoding.ASCII.GetBytes(jwtSecretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var admin = _context.Admins.SingleOrDefault(u => u.Username == request.Username);
        if (admin != null && BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
        {
            var token = GenerateJwtToken(admin.Id.ToString(), admin.Username, admin.Role, key, tokenHandler);
            return Ok(new { Token = token, Username = admin.Username, Role = admin.Role });
        }

        var tenant = await _context.Tenants.SingleOrDefaultAsync(t => t.Username == request.Username);
        if (tenant != null && BCrypt.Net.BCrypt.Verify(request.Password, tenant.PasswordHash))
        {
            var token = GenerateJwtToken(tenant.TenantId.ToString(), tenant.Username, "Tenant", key, tokenHandler);
            return Ok(new { Token = token, Username = tenant.Username, Role = "Tenant" });
        }

        return Unauthorized(new { message = "Invalid username or password" });
    }

    private string GenerateJwtToken(string id, string username, string role, byte[] key, JwtSecurityTokenHandler handler)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
}

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

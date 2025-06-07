using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.Model;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            var admins = await _context.Admins.OfType<Admin>()
                .Select(a => new
                {
                    a.Id,
                    a.Username
                })
                .ToListAsync();
            return Ok(new
            {
                Message = "Get all admins successful",
                Count = admins.Count,
                Data = admins
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdminById(int id)
        {
            var admin = await _context.Admins.OfType<Admin>()
                .Select(a => new 
                {
                    a.Id,
                    a.Username
                })
                .FirstOrDefaultAsync(a => a.Id == id);
            if (admin == null) return NotFound(new { Message = $"Admin with ID {id} not found" });
            return Ok(new
            {
                Message = "Admin found",
                Data = admin
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateAdmin([FromBody] Admin admin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid data", Errors = ModelState });
            }

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);
            admin.Role = "Admin"; 

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAdminById), new { id = admin.Id }, new
            {
                Message = "Admin created successfully",
                Data = admin
            });
        }
    }
}

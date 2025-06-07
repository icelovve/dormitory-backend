using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApplication1.Context;
using WebApplication1.Model;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public TenantController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _dbContext.Tenants
                .Select(t => new
                {
                    t.TenantId,
                    t.Username,
                    t.FullName,
                    t.PhoneNumber,
                    t.Email,
                    t.HouseNumber,
                    t.SubDistrict,
                    t.District,
                    t.Province,
                    t.PostalCode,
                })
                .ToListAsync();

            return Ok(new
            {
                Message = "Get all tenants successful",
                Count = tenants.Count,
                Data = tenants
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTenantById(int id)
        {
            var tenant = await _dbContext.Tenants
                .Select(t => new
                {
                    t.TenantId,
                    t.Username,
                    t.FullName,
                    t.PhoneNumber,
                    t.Email,
                    t.HouseNumber,
                    t.SubDistrict,
                    t.District,
                    t.Province,
                    t.PostalCode,
                })
                .FirstOrDefaultAsync(t => t.TenantId == id);

            if (tenant == null)
            {
                return NotFound(new { Message = $"Tenant with ID {id} not found" });
            }

            return Ok(new
            {
                Message = "Tenant found",
                Data = tenant
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateTenant([FromBody] Model.Tenant tenant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid data", Errors = ModelState });
            }

            tenant.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tenant.PasswordHash);
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTenantById), new { id = tenant.TenantId }, new
            {
                Message = "Tenant created successfully",
                Data = tenant
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTenant(int id, [FromBody] Tenant tenant)
        {
            var existingTenant = await _dbContext.Tenants.FindAsync(id);
            if (existingTenant == null)
                return NotFound(new { Message = $"Tenant with ID {id} not found" });

            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Invalid data", Errors = ModelState });

            existingTenant.FullName = tenant.FullName;
            existingTenant.PhoneNumber = tenant.PhoneNumber;
            existingTenant.HouseNumber = tenant.HouseNumber;
            existingTenant.SubDistrict = tenant.SubDistrict;
            existingTenant.District = tenant.District;
            existingTenant.Province = tenant.Province;
            existingTenant.PostalCode = tenant.PostalCode;
            existingTenant.IDCardNumber = tenant.IDCardNumber;
            existingTenant.Username = tenant.Username;
            existingTenant.Email = tenant.Email;

            if (!string.IsNullOrEmpty(tenant.PasswordHash))
            {
                existingTenant.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tenant.PasswordHash);
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(new
                {
                    Message = "Tenant updated successfully",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Update failed", Error = ex.Message });
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateTenantPartial(int id, [FromBody] JsonElement updateData)
        {
            var tenant = await _dbContext.Tenants.FindAsync(id);
            if (tenant == null)
                return NotFound(new { Message = $"Tenant with ID {id} not found" });

            if (updateData.TryGetProperty("FullName", out var fullName))
                tenant.FullName = fullName.GetString() ?? tenant.FullName;
            if (updateData.TryGetProperty("PhoneNumber", out var phoneNumber))
                tenant.PhoneNumber = phoneNumber.GetString() ?? tenant.PhoneNumber;
            if (updateData.TryGetProperty("Email", out var email))
                tenant.Email = email.GetString() ?? tenant.Email;
            if (updateData.TryGetProperty("HouseNumber", out var houseNumber))
                tenant.HouseNumber = houseNumber.GetString() ?? tenant.HouseNumber;
            if (updateData.TryGetProperty("SubDistrict", out var subDistrict))
                tenant.SubDistrict = subDistrict.GetString() ?? tenant.SubDistrict;
            if (updateData.TryGetProperty("District", out var district))
                tenant.District = district.GetString() ?? tenant.District;
            if (updateData.TryGetProperty("Province", out var province))
                tenant.Province = province.GetString() ?? tenant.Province;
            if (updateData.TryGetProperty("PostalCode", out var postalCode))
                tenant.PostalCode = postalCode.GetString() ?? tenant.PostalCode;
            if (updateData.TryGetProperty("IDCardNumber", out var idCardNumber))
                tenant.IDCardNumber = idCardNumber.GetString() ?? tenant.IDCardNumber;
            if (updateData.TryGetProperty("Username", out var username))
                tenant.Username = username.GetString() ?? tenant.Username;
            if (updateData.TryGetProperty("PasswordHash", out var passwordHash))
            {
                var newPassword = passwordHash.GetString();
                if (!string.IsNullOrEmpty(newPassword))
                {
                    tenant.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(new
                {
                    Message = "Tenant updated successfully",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Update failed", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var tenant = await _dbContext.Tenants.FindAsync(id);
            if (tenant == null)
                return NotFound(new { Message = $"Tenant with ID {id} not found" });

            _dbContext.Tenants.Remove(tenant);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Tenant deleted successfully" });
        }
    }
}
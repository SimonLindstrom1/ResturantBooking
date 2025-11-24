using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBooking.Data;
using RestaurantBooking.Models;
using RestaurantBooking.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public SetupController(RestaurantContext context)
        {
            _context = context;
        }

        /// <summary>
        /// One-time setup endpoint to create the first admin user
        /// This should be disabled after initial setup or protected
        /// </summary>
        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            // Check if any admin already exists
            if (await _context.Administrators.AnyAsync())
            {
                return BadRequest(new { message = "Admin already exists. This endpoint is disabled." });
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Username) || dto.Username.Length < 3)
            {
                return BadRequest(new { message = "Username must be at least 3 characters long" });
            }

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters long" });
            }

            // Create admin user
            var admin = new Administrator
            {
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password),
                Email = dto.Email ?? $"{dto.Username}@restaurant.com",
                CreatedAt = DateTime.UtcNow
            };

            _context.Administrators.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Admin user created successfully",
                username = admin.Username,
                note = "Please login with your credentials. This endpoint is now disabled."
            });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    // DTO for creating admin
    public class CreateAdminDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}
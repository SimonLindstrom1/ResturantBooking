using System.Security.Cryptography;
using System.Text;
using RestaurantBooking.Models;
using RestaurantBooking.Data;
using Microsoft.EntityFrameworkCore;

namespace RestaurantBooking.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(RestaurantContext context)
        {
            // Check if any administrators exist
            if (await context.Administrators.AnyAsync())
            {
                Console.WriteLine("Admin user already exists. Skipping seeding.");
                return;
            }

            // Create default admin user
            var admin = new Administrator
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"), // Default password
                Email = "admin@restaurant.com",
                CreatedAt = DateTime.UtcNow
            };

            context.Administrators.Add(admin);
            await context.SaveChangesAsync();

            Console.WriteLine("Default admin user created successfully!");
            Console.WriteLine("Username: admin");
            Console.WriteLine("Password: admin123");
            Console.WriteLine("IMPORTANT: Change this password after first login!");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
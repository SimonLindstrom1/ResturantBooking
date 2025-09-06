using Microsoft.EntityFrameworkCore;
using RestaurantBooking.Models;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantBooking.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(RestaurantContext context)
        {
            await context.Database.MigrateAsync();

            // Check if admin exists
            if (!await context.Administrators.AnyAsync())
            {
                var admin = new Administrator
                {
                    Username = "admin",
                    PasswordHash = HashPassword("admin123")
                };

                await context.Administrators.AddAsync(admin);
                await context.SaveChangesAsync();
            }

            // Seed menu items if none exist
            if (!await context.MenuItems.AnyAsync())
            {
                var menuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Grilled Salmon",
                        Price = 249.00m,
                        Description = "Fresh Atlantic salmon with herbs",
                        IsPopular = true,
                        ImageUrl = "https://example.com/salmon.jpg"
                    },
                    new MenuItem
                    {
                        Name = "Beef Tenderloin",
                        Price = 329.00m,
                        Description = "Premium beef with garlic butter",
                        IsPopular = true,
                        ImageUrl = "https://example.com/beef.jpg"
                    }
                };

                await context.MenuItems.AddRangeAsync(menuItems);
                await context.SaveChangesAsync();
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

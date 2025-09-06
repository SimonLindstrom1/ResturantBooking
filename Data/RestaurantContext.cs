using Microsoft.EntityFrameworkCore;
using RestaurantBooking.Models;

namespace RestaurantBooking.Data
{
    public class RestaurantContext : DbContext
    {
        public RestaurantContext(DbContextOptions<RestaurantContext> options) : base(options)
        {
        }

        public DbSet<Table> Tables { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Table
            modelBuilder.Entity<Table>()
                .HasIndex(t => t.TableNumber)
                .IsUnique();

            // Configure Administrator
            modelBuilder.Entity<Administrator>()
                .HasIndex(a => a.Username)
                .IsUnique();

            // Configure Booking relationships
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Table)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TableId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            modelBuilder.Entity<Table>().HasData(
                new Table { Id = 1, TableNumber = 1, Capacity = 2 },
                new Table { Id = 2, TableNumber = 2, Capacity = 4 },
                new Table { Id = 3, TableNumber = 3, Capacity = 6 },
                new Table { Id = 4, TableNumber = 4, Capacity = 8 }
            );
        }
    }
}

using Microsoft.EntityFrameworkCore;
using RestaurantBooking.Data;
using RestaurantBooking.DTOs;
using RestaurantBooking.Models;

namespace RestaurantBooking.Services
{
    public class BookingService : IBookingService
    {
        private readonly RestaurantContext _context;

        public BookingService(RestaurantContext context)
        {
            _context = context;
        }

        // ===========================================
        // CREATE BOOKING
        // ===========================================
        public async Task<Booking> CreateBookingAsync(CreateBookingDto dto)
        {
            var start = dto.BookingDate.Date + dto.BookingTime;
            var end = start.AddHours(2);

            // Load ALL bookings for that table (avoid EF DateTime translation issues)
            var tableBookings = await _context.Bookings
                .Where(b => b.TableId == dto.TableId)
                .ToListAsync();

            // Convert existing bookings to real DateTime ranges in memory
            foreach (var b in tableBookings)
            {
                var existingStart = b.BookingDate.Date + b.BookingTime;
                var existingEnd = existingStart.AddHours(2);

                if (existingStart < end && start < existingEnd)
                {
                    throw new InvalidOperationException("Table is not available at this time.");
                }
            }

            // Find or create customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == dto.CustomerPhoneNumber);

            if (customer == null)
            {
                customer = new Customer
                {
                    Name = dto.CustomerName,
                    PhoneNumber = dto.CustomerPhoneNumber
                };

                _context.Customers.Add(customer);
            }

            var booking = new Booking
            {
                BookingDate = dto.BookingDate.Date,
                BookingTime = dto.BookingTime,
                NumberOfGuests = dto.NumberOfGuests,
                TableId = dto.TableId,
                Customer = customer
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return booking;
        }

        // ===========================================
        // GET AVAILABLE TABLES
        // ===========================================
        public async Task<List<AvailableTableDto>> GetAvailableTablesAsync(DateTime date, TimeSpan time, int guests)
        {
            var start = date.Date + time;
            var end = start.AddHours(2);

            // Load tables
            var tables = await _context.Tables
                .Where(t => t.Capacity >= guests)
                .ToListAsync();

            // Load all bookings for the date (raw values)
            var bookings = await _context.Bookings
                .Where(b => b.BookingDate.Date == date.Date)
                .ToListAsync();

            var available = new List<AvailableTableDto>();

            foreach (var table in tables)
            {
                bool conflict = bookings.Any(b =>
                {
                    if (b.TableId != table.Id) return false;

                    var existingStart = b.BookingDate.Date + b.BookingTime;
                    var existingEnd = existingStart.AddHours(2);

                    return existingStart < end && start < existingEnd;
                });

                if (!conflict)
                {
                    available.Add(new AvailableTableDto
                    {
                        Id = table.Id,
                        TableNumber = table.TableNumber,
                        Capacity = table.Capacity
                    });
                }
            }

            return available;
        }
    }
}

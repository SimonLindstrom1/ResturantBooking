using RestaurantBooking.Data;
using RestaurantBooking.DTOs;
using RestaurantBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace RestaurantBooking.Services
{
    public class BookingService : IBookingService
    {
        private readonly RestaurantContext _context;

        public BookingService(RestaurantContext context)
        {
            _context = context;
        }

        public async Task<List<AvailableTableDto>> GetAvailableTablesAsync(DateTime date, TimeSpan time, int numberOfGuests)
        {
            var requestedDateTime = date.Date.Add(time);
            var endTime = requestedDateTime.AddHours(2);

            // Find tables with sufficient capacity
            var suitableTables = await _context.Tables
                .Where(t => t.Capacity >= numberOfGuests)
                .ToListAsync();

            var availableTables = new List<AvailableTableDto>();

            foreach (var table in suitableTables)
            {
                if (await IsTableAvailableAsync(table.Id, date, time))
                {
                    availableTables.Add(new AvailableTableDto
                    {
                        Id = table.Id,
                        TableNumber = table.TableNumber,
                        Capacity = table.Capacity
                    });
                }
            }

            return availableTables;
        }

        public async Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeSpan time)
        {
            var requestedStart = date.Date.Add(time);
            var requestedEnd = requestedStart.AddHours(2);

            // Check for overlapping bookings
            var overlappingBookings = await _context.Bookings
                .Where(b => b.TableId == tableId && b.BookingDate.Date == date.Date)
                .Where(b =>
                    // Existing booking starts before requested end AND ends after requested start
                    b.BookingDate.Add(b.BookingTime) < requestedEnd &&
                    b.BookingDate.Add(b.BookingTime).Add(b.Duration) > requestedStart)
                .AnyAsync();

            return !overlappingBookings;
        }

        public async Task<Booking> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            // Check if table is available
            if (!await IsTableAvailableAsync(bookingDto.TableId, bookingDto.BookingDate, bookingDto.BookingTime))
            {
                throw new InvalidOperationException("Table is not available at the requested time.");
            }

            // Find or create customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == bookingDto.CustomerPhoneNumber);

            if (customer == null)
            {
                customer = new Customer
                {
                    Name = bookingDto.CustomerName,
                    PhoneNumber = bookingDto.CustomerPhoneNumber
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            // Create booking
            var booking = new Booking
            {
                BookingDate = bookingDto.BookingDate,
                BookingTime = bookingDto.BookingTime,
                NumberOfGuests = bookingDto.NumberOfGuests,
                TableId = bookingDto.TableId,
                CustomerId = customer.Id
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return booking;
        }
    }
}

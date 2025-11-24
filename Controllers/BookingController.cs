using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBooking.Data;
using RestaurantBooking.DTOs;
using RestaurantBooking.Services;

namespace RestaurantBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly IBookingService _bookingService;

        public BookingsController(RestaurantContext context, IBookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Table)
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    BookingDate = b.BookingDate,
                    BookingTime = b.BookingTime,
                    NumberOfGuests = b.NumberOfGuests,
                    TableId = b.TableId,
                    Customer = new CustomerDto
                    {
                        Id = b.Customer.Id,
                        Name = b.Customer.Name,
                        PhoneNumber = b.Customer.PhoneNumber
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto bookingDto)
        {
            try
            {
                var booking = await _bookingService.CreateBookingAsync(bookingDto);
                return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Table)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        [HttpGet("available-tables")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableTables([FromQuery] AvailableTablesRequestDto request)
        {
            var availableTables = await _bookingService.GetAvailableTablesAsync(
                request.Date, request.Time, request.NumberOfGuests);

            return Ok(availableTables);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking deleted successfully" });
        }
    }
}


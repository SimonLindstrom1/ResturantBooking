using RestaurantBooking.DTOs;
using RestaurantBooking.Models;

namespace RestaurantBooking.Services
{
    public interface IBookingService
    {
        Task<List<AvailableTableDto>> GetAvailableTablesAsync(DateTime date, TimeSpan time, int numberOfGuests);
        Task<Booking> CreateBookingAsync(CreateBookingDto bookingDto);
    }
}

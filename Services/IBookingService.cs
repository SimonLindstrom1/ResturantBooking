using RestaurantBooking.DTOs;
using RestaurantBooking.Models;

namespace RestaurantBooking.Services
{
    public interface IBookingService
    {
        Task<List<AvailableTableDto>> GetAvailableTablesAsync(DateTime date, TimeSpan time, int numberOfGuests);
        Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeSpan time);
        Task<Booking> CreateBookingAsync(CreateBookingDto bookingDto);
    }
}

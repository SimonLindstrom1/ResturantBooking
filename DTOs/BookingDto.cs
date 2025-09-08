using System.ComponentModel.DataAnnotations;

namespace RestaurantBooking.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public TimeSpan BookingTime { get; set; }

        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        [Required]
        public int TableId { get; set; }

        [Required]
        public CustomerDto Customer { get; set; } = new CustomerDto();
    }

    public class CreateBookingDto
    {
        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public TimeSpan BookingTime { get; set; }

        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        [Required]
        public int TableId { get; set; }

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string CustomerPhoneNumber { get; set; } = string.Empty;
    }
}


using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ResturantBooking.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public TimeSpan BookingTime { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        public int NumberOfGuests { get; set; }

        // Default booking duration is 2 hours
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(2);

        [Required]
        [ForeignKey("Table")]
        public int TableId { get; set; }
        public virtual Table Table { get; set; } = null!;

        [Required]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

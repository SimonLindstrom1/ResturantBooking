using System.ComponentModel.DataAnnotations;

namespace ResturantBooking.Models
{
    public class Table
    {
        public int Id { get; set; }

        [Required]
        public int TableNumber { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20")]
        public int Capacity { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

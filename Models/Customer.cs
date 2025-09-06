using System.ComponentModel.DataAnnotations;

namespace RestaurantBooking.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

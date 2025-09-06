using System.ComponentModel.DataAnnotations;

namespace RestaurantBooking.DTOs
{
    public class TableDto
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public bool IsCurrentlyOccupied { get; set; }
        public int TotalBookings { get; set; }
        public List<BookingDto>? RecentBookings { get; set; }
    }

    public class CreateTableDto
    {
        [Required]
        [Range(1, 999, ErrorMessage = "Table number must be between 1 and 999")]
        public int TableNumber { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20")]
        public int Capacity { get; set; }
    }

    public class UpdateTableDto
    {
        [Required]
        [Range(1, 999, ErrorMessage = "Table number must be between 1 and 999")]
        public int TableNumber { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20")]
        public int Capacity { get; set; }
    }
}

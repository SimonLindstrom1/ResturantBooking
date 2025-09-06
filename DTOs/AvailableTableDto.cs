using System.ComponentModel.DataAnnotations;

namespace RestaurantBooking.DTOs
{
    public class AvailableTablesRequestDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }
    }

    public class AvailableTableDto
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
    }
}


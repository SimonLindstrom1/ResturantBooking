using System.ComponentModel.DataAnnotations;

namespace RestaurantBooking.DTOs
{
    public class MenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CreateMenuItemDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.00")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsPopular { get; set; } = false;

        [Url]
        [StringLength(255)]
        public string? ImageUrl { get; set; }
    }

    public class UpdateMenuItemDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.00")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsPopular { get; set; } = false;

        [Url]
        [StringLength(255)]
        public string? ImageUrl { get; set; }
    }
}

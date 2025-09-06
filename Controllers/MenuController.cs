using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBooking.Data;
using RestaurantBooking.DTOs;
using RestaurantBooking.Models;


namespace RestaurantBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : Controller
    {
        private readonly RestaurantContext _context;

        public MenuController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: api/menu
        [HttpGet]
        [AllowAnonymous] // Public endpoint for customers to view menu
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItems()
        {
            var menuItems = await _context.MenuItems
                .Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Price = m.Price,
                    Description = m.Description,
                    IsPopular = m.IsPopular,
                    ImageUrl = m.ImageUrl
                })
                .ToListAsync();

            return Ok(menuItems);
        }

        // GET: api/menu/popular
        [HttpGet("popular")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetPopularMenuItems()
        {
            var popularItems = await _context.MenuItems
                .Where(m => m.IsPopular)
                .Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Price = m.Price,
                    Description = m.Description,
                    IsPopular = m.IsPopular,
                    ImageUrl = m.ImageUrl
                })
                .ToListAsync();

            return Ok(popularItems);
        }

        // GET: api/menu/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<MenuItemDto>> GetMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);

            if (menuItem == null)
            {
                return NotFound($"Menu item with ID {id} not found.");
            }

            var menuItemDto = new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Price = menuItem.Price,
                Description = menuItem.Description,
                IsPopular = menuItem.IsPopular,
                ImageUrl = menuItem.ImageUrl
            };

            return Ok(menuItemDto);
        }

        // POST: api/menu
        [HttpPost]
        [Authorize] // Only authenticated admins can create menu items
        public async Task<ActionResult<MenuItemDto>> CreateMenuItem(CreateMenuItemDto createMenuItemDto)
        {
            var menuItem = new MenuItem
            {
                Name = createMenuItemDto.Name,
                Price = createMenuItemDto.Price,
                Description = createMenuItemDto.Description,
                IsPopular = createMenuItemDto.IsPopular,
                ImageUrl = createMenuItemDto.ImageUrl
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            var menuItemDto = new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Price = menuItem.Price,
                Description = menuItem.Description,
                IsPopular = menuItem.IsPopular,
                ImageUrl = menuItem.ImageUrl
            };

            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItemDto);
        }

        // PUT: api/menu/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateMenuItem(int id, UpdateMenuItemDto updateMenuItemDto)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);

            if (menuItem == null)
            {
                return NotFound($"Menu item with ID {id} not found.");
            }

            menuItem.Name = updateMenuItemDto.Name;
            menuItem.Price = updateMenuItemDto.Price;
            menuItem.Description = updateMenuItemDto.Description;
            menuItem.IsPopular = updateMenuItemDto.IsPopular;
            menuItem.ImageUrl = updateMenuItemDto.ImageUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await MenuItemExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/menu/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound($"Menu item with ID {id} not found.");
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/menu/5/toggle-popular
        [HttpPatch("{id}/toggle-popular")]
        [Authorize]
        public async Task<IActionResult> TogglePopular(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound($"Menu item with ID {id} not found.");
            }

            menuItem.IsPopular = !menuItem.IsPopular;
            await _context.SaveChangesAsync();

            return Ok(new { IsPopular = menuItem.IsPopular });
        }

        private async Task<bool> MenuItemExists(int id)
        {
            return await _context.MenuItems.AnyAsync(e => e.Id == id);
        }
    }
}

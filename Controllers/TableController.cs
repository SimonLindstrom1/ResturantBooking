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
    [Authorize] // Only authenticated admins can manage tables
    public class TablesController : Controller
    {
        private readonly RestaurantContext _context;

        public TablesController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: api/tables
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TableDto>>> GetTables()
        {
            var tables = await _context.Tables
                .Include(t => t.Bookings)
                .Select(t => new TableDto
                {
                    Id = t.Id,
                    TableNumber = t.TableNumber,
                    Capacity = t.Capacity,
                    IsCurrentlyOccupied = t.Bookings.Any(b =>
                        b.BookingDate.Date == DateTime.Today &&
                        DateTime.Now.TimeOfDay >= b.BookingTime &&
                        DateTime.Now.TimeOfDay <= b.BookingTime.Add(b.Duration)),
                    TotalBookings = t.Bookings.Count
                })
                .OrderBy(t => t.TableNumber)
                .ToListAsync();

            return Ok(tables);
        }

        // GET: api/tables/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TableDto>> GetTable(int id)
        {
            var table = await _context.Tables
                .Include(t => t.Bookings)
                    .ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound($"Table with ID {id} not found.");
            }

            var tableDto = new TableDto
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                IsCurrentlyOccupied = table.Bookings.Any(b =>
                    b.BookingDate.Date == DateTime.Today &&
                    DateTime.Now.TimeOfDay >= b.BookingTime &&
                    DateTime.Now.TimeOfDay <= b.BookingTime.Add(b.Duration)),
                TotalBookings = table.Bookings.Count,
                RecentBookings = table.Bookings
                    .Where(b => b.BookingDate >= DateTime.Today.AddDays(-30))
                    .Select(b => new BookingDto
                    {
                        Id = b.Id,
                        BookingDate = b.BookingDate,
                        BookingTime = b.BookingTime,
                        NumberOfGuests = b.NumberOfGuests,
                        Customer = new CustomerDto
                        {
                            Id = b.Customer.Id,
                            Name = b.Customer.Name,
                            PhoneNumber = b.Customer.PhoneNumber
                        }
                    })
                    .OrderByDescending(b => b.BookingDate)
                    .ThenByDescending(b => b.BookingTime)
                    .Take(10)
                    .ToList()
            };

            return Ok(tableDto);
        }

        // POST: api/tables
        [HttpPost]
        public async Task<ActionResult<TableDto>> CreateTable(CreateTableDto createTableDto)
        {
            // Check if table number already exists
            var existingTable = await _context.Tables
                .FirstOrDefaultAsync(t => t.TableNumber == createTableDto.TableNumber);

            if (existingTable != null)
            {
                return BadRequest($"Table with number {createTableDto.TableNumber} already exists.");
            }

            var table = new Table
            {
                TableNumber = createTableDto.TableNumber,
                Capacity = createTableDto.Capacity
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            var tableDto = new TableDto
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                IsCurrentlyOccupied = false,
                TotalBookings = 0
            };

            return CreatedAtAction(nameof(GetTable), new { id = table.Id }, tableDto);
        }

        // PUT: api/tables/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, UpdateTableDto updateTableDto)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table == null)
            {
                return NotFound($"Table with ID {id} not found.");
            }

            // Check if another table has the same table number
            var existingTable = await _context.Tables
                .FirstOrDefaultAsync(t => t.TableNumber == updateTableDto.TableNumber && t.Id != id);

            if (existingTable != null)
            {
                return BadRequest($"Another table with number {updateTableDto.TableNumber} already exists.");
            }

            table.TableNumber = updateTableDto.TableNumber;
            table.Capacity = updateTableDto.Capacity;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TableExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/tables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var table = await _context.Tables
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound($"Table with ID {id} not found.");
            }

            // Check if table has future bookings
            var futureBookings = table.Bookings
                .Where(b => b.BookingDate.Date.Add(b.BookingTime) >= DateTime.Now)
                .ToList();

            if (futureBookings.Any())
            {
                return BadRequest($"Cannot delete table. Table has {futureBookings.Count} future booking(s).");
            }

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/tables/5/bookings
        [HttpGet("{id}/bookings")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetTableBookings(int id, [FromQuery] DateTime? fromDate = null)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound($"Table with ID {id} not found.");
            }

            var query = _context.Bookings
                .Include(b => b.Customer)
                .Where(b => b.TableId == id);

            if (fromDate.HasValue)
            {
                query = query.Where(b => b.BookingDate >= fromDate.Value.Date);
            }

            var bookings = await query
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    BookingDate = b.BookingDate,
                    BookingTime = b.BookingTime,
                    NumberOfGuests = b.NumberOfGuests,
                    TableId = b.TableId,
                    Customer = new CustomerDto
                    {
                        Id = b.Customer.Id,
                        Name = b.Customer.Name,
                        PhoneNumber = b.Customer.PhoneNumber
                    }
                })
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.BookingTime)
                .ToListAsync();

            return Ok(bookings);
        }

        // GET: api/tables/capacity/{minCapacity}
        [HttpGet("capacity/{minCapacity}")]
        public async Task<ActionResult<IEnumerable<TableDto>>> GetTablesByCapacity(int minCapacity)
        {
            var tables = await _context.Tables
                .Where(t => t.Capacity >= minCapacity)
                .Select(t => new TableDto
                {
                    Id = t.Id,
                    TableNumber = t.TableNumber,
                    Capacity = t.Capacity
                })
                .OrderBy(t => t.TableNumber)
                .ToListAsync();

            return Ok(tables);
        }

        private async Task<bool> TableExists(int id)
        {
            return await _context.Tables.AnyAsync(e => e.Id == id);
        }
    }
}

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
    [Authorize] // Only authenticated admins can manage customers
    public class CustomersController : Controller
    {
        private readonly RestaurantContext _context;

        public CustomersController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: api/customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            var customers = await _context.Customers
                .Include(c => c.Bookings)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    BookingCount = c.Bookings.Count
                })
                .ToListAsync();

            return Ok(customers);
        }

        // GET: api/customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Bookings)
                    .ThenInclude(b => b.Table)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                BookingCount = customer.Bookings.Count,
                Bookings = customer.Bookings.Select(b => new BookingDto
                {
                    Id = b.Id,
                    BookingDate = b.BookingDate,
                    BookingTime = b.BookingTime,
                    NumberOfGuests = b.NumberOfGuests,
                    TableId = b.TableId
                }).ToList()
            };

            return Ok(customerDto);
        }

        // POST: api/customers
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto)
        {
            // Check if customer with same phone number already exists
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == createCustomerDto.PhoneNumber);

            if (existingCustomer != null)
            {
                return BadRequest($"Customer with phone number {createCustomerDto.PhoneNumber} already exists.");
            }

            var customer = new Customer
            {
                Name = createCustomerDto.Name,
                PhoneNumber = createCustomerDto.PhoneNumber
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                BookingCount = 0
            };

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customerDto);
        }

        // PUT: api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerDto updateCustomerDto)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            // Check if another customer has the same phone number
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == updateCustomerDto.PhoneNumber && c.Id != id);

            if (existingCustomer != null)
            {
                return BadRequest($"Another customer with phone number {updateCustomerDto.PhoneNumber} already exists.");
            }

            customer.Name = updateCustomerDto.Name;
            customer.PhoneNumber = updateCustomerDto.PhoneNumber;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CustomerExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            // Check if customer has active bookings
            var activeBookings = customer.Bookings
                .Where(b => b.BookingDate.Date.Add(b.BookingTime) >= DateTime.Now)
                .ToList();

            if (activeBookings.Any())
            {
                return BadRequest($"Cannot delete customer. Customer has {activeBookings.Count} active booking(s).");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/customers/search?phone=123456789
        [HttpGet("search")]
        public async Task<ActionResult<CustomerDto>> SearchCustomerByPhone([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return BadRequest("Phone number is required.");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == phone);

            if (customer == null)
            {
                return NotFound($"Customer with phone number {phone} not found.");
            }

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber
            };

            return Ok(customerDto);
        }

        private async Task<bool> CustomerExists(int id)
        {
            return await _context.Customers.AnyAsync(e => e.Id == id);
        }
    }
}

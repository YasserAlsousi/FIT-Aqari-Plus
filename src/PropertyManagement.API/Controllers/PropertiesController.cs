using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PropertiesController : ControllerBase
    {
        private readonly PropertyManagementDbContext _context;

        public PropertiesController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Property>>> GetProperties(
            [FromQuery] string? type = null,
            [FromQuery] string? status = null,
            [FromQuery] string? city = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Properties
                .Include(p => p.Owner)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(p => p.PropertyType == type);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(p => p.City.Contains(city));

            if (minPrice.HasValue)
                query = query.Where(p => p.MonthlyRent >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.MonthlyRent <= maxPrice.Value);

            var totalCount = await query.CountAsync();
            var properties = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(properties);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Property>> GetProperty(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Owner)
                .Include(p => p.Contracts)
                    .ThenInclude(c => c.Tenant)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
                return NotFound();

            return Ok(property);
        }

        [HttpPost]
        public async Task<ActionResult<Property>> CreateProperty(Property property)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            property.CreatedAt = DateTime.UtcNow;
            property.UpdatedAt = DateTime.UtcNow;

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProperty), new { id = property.Id }, property);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProperty(int id, Property property)
        {
            if (id != property.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            property.UpdatedAt = DateTime.UtcNow;
            _context.Entry(property).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PropertyExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
                return NotFound();

            // Check if property has active contracts
            var hasActiveContracts = await _context.Contracts
                .AnyAsync(c => c.PropertyId == id && c.IsActive);

            if (hasActiveContracts)
                return BadRequest("Cannot delete property with active contracts");

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Property>>> SearchProperties([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
                return BadRequest("Search query is required");

            var properties = await _context.Properties
                .Include(p => p.Owner)
                .Where(p => p.Title.Contains(query) ||
                           p.Description.Contains(query) ||
                           p.Address.Contains(query) ||
                           p.City.Contains(query))
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();

            return Ok(properties);
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
    }
}

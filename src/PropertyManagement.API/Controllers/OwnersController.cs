using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OwnersController : ControllerBase
    {
        private readonly PropertyManagementDbContext _context;

        public OwnersController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Owner>>> GetOwners(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Owners
                .Include(o => o.Properties)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.FirstName.Contains(search) ||
                                        o.LastName.Contains(search) ||
                                        o.Email.Contains(search) ||
                                        o.Phone.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var owners = await query
                .OrderBy(o => o.FirstName)
                .ThenBy(o => o.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page", page.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());

            return Ok(owners);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Owner>> GetOwner(int id)
        {
            var owner = await _context.Owners
                .Include(o => o.Properties)
                    .ThenInclude(p => p.Contracts)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (owner == null)
                return NotFound();

            return Ok(owner);
        }

        [HttpPost]
        public async Task<ActionResult<Owner>> CreateOwner(Owner owner)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicate email
            if (await _context.Owners.AnyAsync(o => o.Email == owner.Email))
                return BadRequest("Owner with this email already exists");

            owner.CreatedAt = DateTime.UtcNow;
            _context.Owners.Add(owner);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOwner), new { id = owner.Id }, owner);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOwner(int id, Owner owner)
        {
            if (id != owner.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicate email (excluding current owner)
            if (await _context.Owners.AnyAsync(o => o.Email == owner.Email && o.Id != id))
                return BadRequest("Owner with this email already exists");

            _context.Entry(owner).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OwnerExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOwner(int id)
        {
            var owner = await _context.Owners
                .Include(o => o.Properties)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (owner == null)
                return NotFound();

            // Check if owner has properties
            if (owner.Properties.Any())
                return BadRequest("Cannot delete owner with existing properties");

            _context.Owners.Remove(owner);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/properties")]
        public async Task<ActionResult<IEnumerable<Property>>> GetOwnerProperties(int id)
        {
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
                return NotFound();

            var properties = await _context.Properties
                .Include(p => p.Contracts)
                .Where(p => p.OwnerId == id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(properties);
        }

        private bool OwnerExists(int id)
        {
            return _context.Owners.Any(e => e.Id == id);
        }
    }
}

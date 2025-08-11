using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly PropertyManagementDbContext _context;

        public TenantsController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tenant>>> GetTenants(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Tenants
                .Include(t => t.Contracts)
                    .ThenInclude(c => c.Property)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.FirstName.Contains(search) ||
                                        t.LastName.Contains(search) ||
                                        t.Email.Contains(search) ||
                                        t.Phone.Contains(search) ||
                                        t.NationalId.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var tenants = await query
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page", page.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());

            return Ok(tenants);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tenant>> GetTenant(int id)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Contracts)
                    .ThenInclude(c => c.Property)
                        .ThenInclude(p => p.Owner)
                .Include(t => t.Contracts)
                    .ThenInclude(c => c.Payments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
                return NotFound();

            return Ok(tenant);
        }

        [HttpPost]
        public async Task<ActionResult<Tenant>> CreateTenant(Tenant tenant)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicate email
            if (await _context.Tenants.AnyAsync(t => t.Email == tenant.Email))
                return BadRequest("Tenant with this email already exists");

            // Check for duplicate national ID
            if (!string.IsNullOrEmpty(tenant.NationalId) && 
                await _context.Tenants.AnyAsync(t => t.NationalId == tenant.NationalId))
                return BadRequest("Tenant with this National ID already exists");

            tenant.CreatedAt = DateTime.UtcNow;
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTenant(int id, Tenant tenant)
        {
            if (id != tenant.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicate email (excluding current tenant)
            if (await _context.Tenants.AnyAsync(t => t.Email == tenant.Email && t.Id != id))
                return BadRequest("Tenant with this email already exists");

            // Check for duplicate national ID (excluding current tenant)
            if (!string.IsNullOrEmpty(tenant.NationalId) && 
                await _context.Tenants.AnyAsync(t => t.NationalId == tenant.NationalId && t.Id != id))
                return BadRequest("Tenant with this National ID already exists");

            _context.Entry(tenant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Contracts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
                return NotFound();

            // Check if tenant has active contracts
            if (tenant.Contracts.Any(c => c.Status == "Active"))
                return BadRequest("Cannot delete tenant with active contracts");

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/contracts")]
        public async Task<ActionResult<IEnumerable<Contract>>> GetTenantContracts(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
                return NotFound();

            var contracts = await _context.Contracts
                .Include(c => c.Property)
                    .ThenInclude(p => p.Owner)
                .Include(c => c.Payments)
                .Where(c => c.TenantId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(contracts);
        }

        [HttpGet("{id}/payments")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetTenantPayments(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
                return NotFound();

            var payments = await _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Property)
                .Where(p => p.Contract.TenantId == id)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();

            return Ok(payments);
        }

        private bool TenantExists(int id)
        {
            return _context.Tenants.Any(e => e.Id == id);
        }
    }
}

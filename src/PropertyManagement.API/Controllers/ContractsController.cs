using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly PropertyManagementDbContext _context;

        public ContractsController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contract>>> GetContracts(
            [FromQuery] string? status = null,
            [FromQuery] int? propertyId = null,
            [FromQuery] int? tenantId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Contracts
                .Include(c => c.Property)
                    .ThenInclude(p => p.Owner)
                .Include(c => c.Tenant)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (propertyId.HasValue)
                query = query.Where(c => c.PropertyId == propertyId.Value);

            if (tenantId.HasValue)
                query = query.Where(c => c.TenantId == tenantId.Value);

            var totalCount = await query.CountAsync();
            var contracts = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());

            return Ok(contracts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Contract>> GetContract(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Property)
                    .ThenInclude(p => p.Owner)
                .Include(c => c.Tenant)
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            return Ok(contract);
        }

        [HttpPost]
        public async Task<ActionResult<Contract>> CreateContract(Contract contract)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Generate contract number
            var contractCount = await _context.Contracts.CountAsync();
            contract.ContractNumber = $"CON-{DateTime.Now.Year}-{(contractCount + 1):D6}";
            contract.CreatedAt = DateTime.UtcNow;

            // Update property status
            var property = await _context.Properties.FindAsync(contract.PropertyId);
            if (property != null)
            {
                property.Status = "Rented";
                property.UpdatedAt = DateTime.UtcNow;
            }

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contract);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(int id, Contract contract)
        {
            if (id != contract.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(contract).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContractExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPut("{id}/terminate")]
        public async Task<IActionResult> TerminateContract(int id, [FromBody] string reason)
        {
            var contract = await _context.Contracts
                .Include(c => c.Property)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            contract.Status = "Terminated";
            contract.Notes += $"\nTerminated on {DateTime.UtcNow:yyyy-MM-dd}: {reason}";

            // Update property status
            if (contract.Property != null)
            {
                contract.Property.Status = "Available";
                contract.Property.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/generate-payments")]
        public async Task<IActionResult> GeneratePayments(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            // Check if payments already exist
            var existingPayments = await _context.Payments
                .AnyAsync(p => p.ContractId == id);

            if (existingPayments)
                return BadRequest("Payments already generated for this contract");

            var payments = new List<Payment>();
            var currentDate = contract.StartDate;
            var paymentNumber = 1;

            while (currentDate <= contract.EndDate)
            {
                var payment = new Payment
                {
                    ReceiptNumber = $"{contract.ContractNumber}-P{paymentNumber:D3}",
                    ContractId = contract.Id,
                    PaymentType = "Rent",
                    Status = "Pending",
                    Amount = contract.MonthlyRent,
                    DueDate = currentDate,
                    Notes = $"Monthly rent for {currentDate:MMMM yyyy}",
                    CreatedAt = DateTime.UtcNow
                };

                payments.Add(payment);
                currentDate = currentDate.AddMonths(1); // Monthly payments
                paymentNumber++;
            }

            _context.Payments.AddRange(payments);
            await _context.SaveChangesAsync();

            return Ok($"Generated {payments.Count} payments");
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }
    }
}

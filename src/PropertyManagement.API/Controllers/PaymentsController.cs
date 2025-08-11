using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;
using PropertyManagement.API.DTOs;

namespace PropertyManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PropertyManagementDbContext _context;
        
        public PaymentsController(PropertyManagementDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments(
            [FromQuery] string? status = null,
            [FromQuery] int? contractId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Property)
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Tenant)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (contractId.HasValue)
                query = query.Where(p => p.ContractId == contractId.Value);

            if (fromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaymentDate <= toDate.Value);

            var payments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return Ok(payments);
        }
        
        [HttpPost]
        public async Task<ActionResult<Payment>> CreatePayment(Payment payment)
        {
            payment.ReceiptNumber = await GenerateReceiptNumber();
            payment.CreatedAt = DateTime.UtcNow;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Property)
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Tenant)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (payment == null)
                return NotFound();
                
            return Ok(payment);
        }
        
        [HttpPut("{id}/mark-paid")]
        public async Task<IActionResult> MarkAsPaid(int id, [FromBody] MarkPaidRequest request)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = "Paid";
            payment.PaymentDate = DateTime.UtcNow;
            payment.TransactionReference = request.TransactionReference;
            payment.PaymentMethod = request.PaymentMethod;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/mark-overdue")]
        public async Task<IActionResult> MarkAsOverdue(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = "Overdue";
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, Payment payment)
        {
            if (id != payment.Id)
                return BadRequest();

            var existingPayment = await _context.Payments.FindAsync(id);
            if (existingPayment == null)
                return NotFound();

            existingPayment.Amount = payment.Amount;
            existingPayment.DueDate = payment.DueDate;
            existingPayment.PaymentType = payment.PaymentType;
            existingPayment.Notes = payment.Notes;
            existingPayment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            // Only allow deletion if payment is not paid
            if (payment.Status == "Paid")
                return BadRequest("Cannot delete a paid payment");

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<PaymentStatistics>> GetPaymentStatistics()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var statistics = new PaymentStatistics
            {
                TotalPayments = await _context.Payments.CountAsync(),
                PaidPayments = await _context.Payments.CountAsync(p => p.Status == "Paid"),
                PendingPayments = await _context.Payments.CountAsync(p => p.Status == "Pending"),
                OverduePayments = await _context.Payments.CountAsync(p => p.Status == "Overdue"),

                MonthlyRevenue = await _context.Payments
                    .Where(p => p.Status == "Paid" &&
                               p.PaymentDate.HasValue &&
                               p.PaymentDate.Value.Month == currentMonth &&
                               p.PaymentDate.Value.Year == currentYear)
                    .SumAsync(p => p.Amount),

                YearlyRevenue = await _context.Payments
                    .Where(p => p.Status == "Paid" &&
                               p.PaymentDate.HasValue &&
                               p.PaymentDate.Value.Year == currentYear)
                    .SumAsync(p => p.Amount),

                TotalOutstanding = await _context.Payments
                    .Where(p => p.Status == "Pending" || p.Status == "Overdue")
                    .SumAsync(p => p.Amount)
            };

            return Ok(statistics);
        }
        
        private async Task<string> GenerateReceiptNumber()
        {
            var year = DateTime.Now.Year;
            var count = await _context.Payments
                .CountAsync(p => p.CreatedAt.Year == year);

            return $"REC-{year}-{(count + 1):D6}";
        }
    }
}
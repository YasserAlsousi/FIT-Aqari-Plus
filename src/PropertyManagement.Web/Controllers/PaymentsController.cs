using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.Web.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly PropertyManagementDbContext _context;

        public PaymentsController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, string status, int? month, int page = 1)
        {
            var query = _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Property)
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Tenant)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.ReceiptNumber.Contains(search) ||
                                        p.Contract.ContractNumber.Contains(search) ||
                                        p.Contract.Tenant.FirstName.Contains(search) ||
                                        p.Contract.Tenant.LastName.Contains(search));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (month.HasValue)
                query = query.Where(p => p.DueDate.Month == month.Value);

            var pageSize = 10;
            var totalCount = await query.CountAsync();
            var payments = await query
                .OrderByDescending(p => p.DueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate summary data
            ViewBag.TotalReceived = await _context.Payments
                .Where(p => p.Status == "Paid")
                .SumAsync(p => p.Amount);
            ViewBag.PendingPayments = await _context.Payments
                .CountAsync(p => p.Status == "Pending");
            ViewBag.ThisMonthPayments = await _context.Payments
                .CountAsync(p => p.DueDate.Month == DateTime.Now.Month && p.DueDate.Year == DateTime.Now.Year);
            ViewBag.OverduePayments = await _context.Payments
                .CountAsync(p => p.Status == "Overdue");

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(payments);
        }

        public async Task<IActionResult> Details(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Property)
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Tenant)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        public async Task<IActionResult> Create()
        {
            var contracts = await _context.Contracts
                .Where(c => c.IsActive)
                .Include(c => c.Property)
                .Include(c => c.Tenant)
                .ToListAsync();

            ViewBag.Contracts = new SelectList(contracts, "Id", "ContractNumber");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                // Generate receipt number if not provided
                if (string.IsNullOrEmpty(payment.ReceiptNumber))
                {
                    var lastPayment = await _context.Payments
                        .OrderByDescending(p => p.Id)
                        .FirstOrDefaultAsync();

                    payment.ReceiptNumber = $"RCP-{DateTime.Now:yyyy}-{(lastPayment?.Id + 1 ?? 1):D3}";
                }

                payment.CreatedAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(payment.Status))
                    payment.Status = "Pending";

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إنشاء المدفوعة بنجاح";
                return RedirectToAction(nameof(Index));
            }

            var contracts = await _context.Contracts
                .Where(c => c.IsActive)
                .Include(c => c.Property)
                .Include(c => c.Tenant)
                .ToListAsync();

            ViewBag.Contracts = new SelectList(contracts, "Id", "ContractNumber");
            return View(payment);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            var contracts = await _context.Contracts
                .Include(c => c.Property)
                .Include(c => c.Tenant)
                .ToListAsync();

            ViewBag.Contracts = new SelectList(contracts, "Id", "ContractNumber", payment.ContractId);
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payment payment)
        {
            if (id != payment.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    payment.UpdatedAt = DateTime.UtcNow;
                    _context.Update(payment);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "تم تحديث المدفوعة بنجاح";
                    return RedirectToAction(nameof(Details), new { id = payment.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id))
                        return NotFound();
                    throw;
                }
            }

            var contracts = await _context.Contracts
                .Include(c => c.Property)
                .Include(c => c.Tenant)
                .ToListAsync();

            ViewBag.Contracts = new SelectList(contracts, "Id", "ContractNumber", payment.ContractId);
            return View(payment);
        }

        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = "Paid";
            payment.PaymentDate = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تسجيل الدفع بنجاح";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id, string? transactionReference)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = "Paid";
            payment.PaymentDate = DateTime.UtcNow;
            payment.TransactionReference = transactionReference ?? "";
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تسجيل الدفع بنجاح";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            // Only allow deletion if payment is not paid
            if (payment.Status == "Paid")
            {
                TempData["Error"] = "لا يمكن حذف مدفوعة تم دفعها بالفعل";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "تم حذف المدفوعة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Property)
                        .ThenInclude(p => p.Owner)
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Tenant)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}

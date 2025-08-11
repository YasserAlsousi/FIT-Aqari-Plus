using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.Web.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly PropertyManagementDbContext _context;

        public MaintenanceController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? status, int? priority, int page = 1, int pageSize = 10)
        {
            var query = _context.MaintenanceRequests
                .Include(m => m.Property)
                    .ThenInclude(p => p.Owner)
                .Include(m => m.Tenant)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(m => m.Status == (MaintenanceStatus)status.Value);

            if (priority.HasValue)
                query = query.Where(m => m.Priority == (MaintenancePriority)priority.Value);

            var totalCount = await query.CountAsync();
            var requests = await query
                .OrderByDescending(m => m.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.Priority = priority;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(requests);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.MaintenanceRequests
                .Include(m => m.Property)
                    .ThenInclude(p => p.Owner)
                .Include(m => m.Tenant)
                .Include(m => m.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null)
                return NotFound();

            return View(request);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Properties = await _context.Properties
                .Include(p => p.Owner)
                .Select(p => new { p.Id, p.Title, p.Address })
                .ToListAsync();
            ViewBag.Tenants = await _context.Tenants
                .Select(t => new { t.Id, Name = t.FirstName + " " + t.LastName })
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceRequest request)
        {
            if (ModelState.IsValid)
            {
                // Generate request number
                var lastRequest = await _context.MaintenanceRequests
                    .OrderByDescending(m => m.Id)
                    .FirstOrDefaultAsync();

                var requestNumber = $"MR{DateTime.Now:yyyyMM}{(lastRequest?.Id + 1 ?? 1):D4}";
                request.RequestNumber = requestNumber;
                request.RequestDate = DateTime.UtcNow;
                request.Status = MaintenanceStatus.Submitted;

                _context.MaintenanceRequests.Add(request);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إنشاء طلب الصيانة بنجاح";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Properties = await _context.Properties
                .Include(p => p.Owner)
                .Select(p => new { p.Id, p.Title, p.Address })
                .ToListAsync();
            ViewBag.Tenants = await _context.Tenants
                .Select(t => new { t.Id, Name = t.FirstName + " " + t.LastName })
                .ToListAsync();
            return View(request);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            ViewBag.Properties = await _context.Properties
                .Include(p => p.Owner)
                .ToListAsync();
            ViewBag.Tenants = await _context.Tenants.ToListAsync();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceRequest request)
        {
            if (id != request.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(request);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "تم تحديث طلب الصيانة بنجاح";
                    return RedirectToAction(nameof(Details), new { id = request.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceRequestExists(request.Id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Properties = await _context.Properties
                .Include(p => p.Owner)
                .ToListAsync();
            ViewBag.Tenants = await _context.Tenants.ToListAsync();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, decimal? actualCost, string? completionNotes)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.Status = MaintenanceStatus.Completed;
            request.CompletedDate = DateTime.UtcNow;
            request.ActualCost = actualCost;
            request.CompletionNotes = completionNotes ?? "";

            await _context.SaveChangesAsync();
            
            TempData["Success"] = "تم إكمال طلب الصيانة بنجاح";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            // Only allow deletion if status is Submitted or Cancelled
            if (request.Status != MaintenanceStatus.Submitted && request.Status != MaintenanceStatus.Cancelled)
            {
                TempData["Error"] = "لا يمكن حذف طلب صيانة قيد التنفيذ أو مكتمل";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.MaintenanceRequests.Remove(request);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "تم حذف طلب الصيانة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private bool MaintenanceRequestExists(int id)
        {
            return _context.MaintenanceRequests.Any(e => e.Id == id);
        }
    }
}

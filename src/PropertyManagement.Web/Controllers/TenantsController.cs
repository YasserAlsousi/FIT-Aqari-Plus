using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.Web.Controllers
{
    public class TenantsController : Controller
    {
        private readonly PropertyManagementDbContext _context;

        public TenantsController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
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

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(tenants);
        }

        public async Task<IActionResult> Details(int id)
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

            return View(tenant);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate email
                if (await _context.Tenants.AnyAsync(t => t.Email == tenant.Email))
                {
                    ModelState.AddModelError("Email", "مستأجر بهذا البريد الإلكتروني موجود بالفعل");
                    return View(tenant);
                }

                // Check for duplicate national ID
                if (!string.IsNullOrEmpty(tenant.NationalId) && 
                    await _context.Tenants.AnyAsync(t => t.NationalId == tenant.NationalId))
                {
                    ModelState.AddModelError("NationalId", "مستأجر بهذا الرقم القومي موجود بالفعل");
                    return View(tenant);
                }

                tenant.CreatedAt = DateTime.UtcNow;
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "تم إضافة المستأجر بنجاح";
                return RedirectToAction(nameof(Details), new { id = tenant.Id });
            }

            return View(tenant);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
                return NotFound();

            return View(tenant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tenant tenant)
        {
            if (id != tenant.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                // Check for duplicate email (excluding current tenant)
                if (await _context.Tenants.AnyAsync(t => t.Email == tenant.Email && t.Id != id))
                {
                    ModelState.AddModelError("Email", "مستأجر بهذا البريد الإلكتروني موجود بالفعل");
                    return View(tenant);
                }

                // Check for duplicate national ID (excluding current tenant)
                if (!string.IsNullOrEmpty(tenant.NationalId) && 
                    await _context.Tenants.AnyAsync(t => t.NationalId == tenant.NationalId && t.Id != id))
                {
                    ModelState.AddModelError("NationalId", "مستأجر بهذا الرقم القومي موجود بالفعل");
                    return View(tenant);
                }

                try
                {
                    _context.Update(tenant);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "تم تحديث بيانات المستأجر بنجاح";
                    return RedirectToAction(nameof(Details), new { id = tenant.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TenantExists(tenant.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(tenant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Contracts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
                return NotFound();

            // Check if tenant has active contracts
            if (tenant.Contracts.Any(c => c.IsActive))
            {
                TempData["Error"] = "لا يمكن حذف مستأجر له عقود نشطة";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "تم حذف المستأجر بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private bool TenantExists(int id)
        {
            return _context.Tenants.Any(e => e.Id == id);
        }
    }
}

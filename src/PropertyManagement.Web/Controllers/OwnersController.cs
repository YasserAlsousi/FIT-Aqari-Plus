using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.Web.Controllers
{
    public class OwnersController : Controller
    {
        private readonly PropertyManagementDbContext _context;

        public OwnersController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
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

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(owners);
        }

        public async Task<IActionResult> Details(int id)
        {
            var owner = await _context.Owners
                .Include(o => o.Properties)
                    .ThenInclude(p => p.Contracts)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (owner == null)
                return NotFound();

            return View(owner);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Owner owner)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate email
                if (await _context.Owners.AnyAsync(o => o.Email == owner.Email))
                {
                    ModelState.AddModelError("Email", "مالك بهذا البريد الإلكتروني موجود بالفعل");
                    return View(owner);
                }

                owner.CreatedAt = DateTime.UtcNow;
                _context.Owners.Add(owner);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "تم إضافة المالك بنجاح";
                return RedirectToAction(nameof(Details), new { id = owner.Id });
            }

            return View(owner);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
                return NotFound();

            return View(owner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Owner owner)
        {
            if (id != owner.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                // Check for duplicate email (excluding current owner)
                if (await _context.Owners.AnyAsync(o => o.Email == owner.Email && o.Id != id))
                {
                    ModelState.AddModelError("Email", "مالك بهذا البريد الإلكتروني موجود بالفعل");
                    return View(owner);
                }

                try
                {
                    _context.Update(owner);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "تم تحديث بيانات المالك بنجاح";
                    return RedirectToAction(nameof(Details), new { id = owner.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OwnerExists(owner.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(owner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var owner = await _context.Owners
                .Include(o => o.Properties)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (owner == null)
                return NotFound();

            // Check if owner has properties
            if (owner.Properties.Any())
            {
                TempData["Error"] = "لا يمكن حذف مالك له عقارات مسجلة";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Owners.Remove(owner);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "تم حذف المالك بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private bool OwnerExists(int id)
        {
            return _context.Owners.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.Web.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly PropertyManagementDbContext _context;

        public PropertiesController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, string type, string status, int page = 1)
        {
            var query = _context.Properties
                .Include(p => p.Owner)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Title.Contains(search) ||
                                        p.Description.Contains(search) ||
                                        p.Address.Contains(search));

            if (!string.IsNullOrEmpty(type))
                query = query.Where(p => p.PropertyType == type);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            var pageSize = 12;
            var totalCount = await query.CountAsync();
            var properties = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(properties);
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Owner)
                .Include(p => p.Contracts)
                    .ThenInclude(c => c.Tenant)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
                return NotFound();

            return View(property);
        }

        public async Task<IActionResult> Create()
        {
            var owners = await _context.Owners.ToListAsync();
            ViewBag.Owners = new SelectList(owners, "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property)
        {
            if (ModelState.IsValid)
            {
                property.CreatedAt = DateTime.UtcNow;
                property.UpdatedAt = DateTime.UtcNow;

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة العقار بنجاح";
                return RedirectToAction(nameof(Index));
            }

            var owners = await _context.Owners.ToListAsync();
            ViewBag.Owners = new SelectList(owners, "Id", "FirstName");
            return View(property);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
                return NotFound();

            var owners = await _context.Owners.ToListAsync();
            ViewBag.Owners = new SelectList(owners, "Id", "FirstName", property.OwnerId);
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Property property)
        {
            if (id != property.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    property.UpdatedAt = DateTime.UtcNow;
                    _context.Update(property);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "تم تحديث العقار بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(property.Id))
                        return NotFound();
                    throw;
                }
            }

            var owners = await _context.Owners.ToListAsync();
            ViewBag.Owners = new SelectList(owners, "Id", "FirstName", property.OwnerId);
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
                return NotFound();

            // Check if property has active contracts
            var hasActiveContracts = await _context.Contracts
                .AnyAsync(c => c.PropertyId == id && c.IsActive);

            if (hasActiveContracts)
            {
                TempData["Error"] = "لا يمكن حذف عقار له عقود نشطة";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف العقار بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
    }
}
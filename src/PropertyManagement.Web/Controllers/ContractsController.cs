using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Core.Entities;
using PropertyManagement.Infrastructure.Data;

namespace PropertyManagement.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly PropertyManagementDbContext _context;

        public ContractsController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, bool? active, int page = 1)
        {
            var query = _context.Contracts
                .Include(c => c.Property)
                    .ThenInclude(p => p.Owner)
                .Include(c => c.Tenant)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.ContractNumber.Contains(search) ||
                                        c.Property.Title.Contains(search) ||
                                        c.Tenant.FirstName.Contains(search) ||
                                        c.Tenant.LastName.Contains(search));

            if (active.HasValue)
                query = query.Where(c => c.IsActive == active.Value);

            var pageSize = 10;
            var totalCount = await query.CountAsync();
            var contracts = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(contracts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Property)
                    .ThenInclude(p => p.Owner)
                .Include(c => c.Tenant)
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            return View(contract);
        }

        public async Task<IActionResult> Create()
        {
            var properties = await _context.Properties
                .Where(p => p.Status == "Available")
                .Include(p => p.Owner)
                .ToListAsync();

            var tenants = await _context.Tenants.ToListAsync();

            ViewBag.Properties = new SelectList(properties, "Id", "Title");
            ViewBag.Tenants = new SelectList(tenants, "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            if (ModelState.IsValid)
            {
                // Generate contract number if not provided
                if (string.IsNullOrEmpty(contract.ContractNumber))
                {
                    var lastContract = await _context.Contracts
                        .OrderByDescending(c => c.Id)
                        .FirstOrDefaultAsync();

                    contract.ContractNumber = $"CNT-{DateTime.Now:yyyy}-{(lastContract?.Id + 1 ?? 1):D3}";
                }

                contract.CreatedAt = DateTime.UtcNow;
                contract.UpdatedAt = DateTime.UtcNow;

                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();

                // Update property status
                var property = await _context.Properties.FindAsync(contract.PropertyId);
                if (property != null)
                {
                    property.Status = "Rented";
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "تم إنشاء العقد بنجاح";
                return RedirectToAction(nameof(Index));
            }

            var properties = await _context.Properties
                .Where(p => p.Status == "Available")
                .Include(p => p.Owner)
                .ToListAsync();

            var tenants = await _context.Tenants.ToListAsync();

            ViewBag.Properties = new SelectList(properties, "Id", "Title");
            ViewBag.Tenants = new SelectList(tenants, "Id", "FirstName");
            return View(contract);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            var properties = await _context.Properties.Include(p => p.Owner).ToListAsync();
            var tenants = await _context.Tenants.ToListAsync();

            ViewBag.Properties = new SelectList(properties, "Id", "Title", contract.PropertyId);
            ViewBag.Tenants = new SelectList(tenants, "Id", "FirstName", contract.TenantId);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (id != contract.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    contract.UpdatedAt = DateTime.UtcNow;
                    _context.Update(contract);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "تم تحديث العقد بنجاح";
                    return RedirectToAction(nameof(Details), new { id = contract.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
                        return NotFound();
                    throw;
                }
            }

            var properties = await _context.Properties.Include(p => p.Owner).ToListAsync();
            var tenants = await _context.Tenants.ToListAsync();

            ViewBag.Properties = new SelectList(properties, "Id", "Title", contract.PropertyId);
            ViewBag.Tenants = new SelectList(tenants, "Id", "FirstName", contract.TenantId);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Terminate(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Property)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            contract.IsActive = false;
            contract.UpdatedAt = DateTime.UtcNow;

            // Update property status back to available
            if (contract.Property != null)
            {
                contract.Property.Status = "Available";
                contract.Property.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إنهاء العقد بنجاح";
            return RedirectToAction(nameof(Details), new { id });
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Infrastructure.Data;
using PropertyManagement.Web.Models;
using System.Diagnostics;

namespace PropertyManagement.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly PropertyManagementDbContext _context;

        public HomeController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalProperties = await _context.Properties.CountAsync();
            var availableProperties = await _context.Properties.CountAsync(p => p.Status == "Available");
            var rentedProperties = await _context.Properties.CountAsync(p => p.Status == "Rented");

            var dashboard = new DashboardViewModel
            {
                TotalProperties = totalProperties,
                AvailableProperties = availableProperties,
                RentedProperties = rentedProperties,
                ActiveContracts = await _context.Contracts.CountAsync(c => c.IsActive),
                PendingPayments = await _context.Payments.CountAsync(p => p.Status == "Pending"),
                OverduePayments = await _context.Payments.CountAsync(p => p.Status == "Overdue"),
                MaintenanceRequests = 0, // Will be implemented later

                OccupancyRate = totalProperties > 0 ? (decimal)rentedProperties / totalProperties * 100 : 0,
                MonthlyRevenue = await CalculateMonthlyRevenue(),
                RecentProperties = await _context.Properties
                    .Include(p => p.Owner)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                RecentPayments = await _context.Payments
                    .Include(p => p.Contract)
                        .ThenInclude(c => c.Property)
                    .Include(p => p.Contract)
                        .ThenInclude(c => c.Tenant)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboard);
        }

        private async Task<decimal> CalculateMonthlyRevenue()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            return await _context.Payments
                .Where(p => p.Status == "Paid" &&
                           p.PaymentDate.HasValue &&
                           p.PaymentDate.Value.Month == currentMonth &&
                           p.PaymentDate.Value.Year == currentYear)
                .SumAsync(p => p.Amount);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

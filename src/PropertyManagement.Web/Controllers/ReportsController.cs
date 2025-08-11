using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Infrastructure.Data;
using PropertyManagement.Web.Models;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly PropertyManagementDbContext _context;

        public ReportsController(PropertyManagementDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ReportsViewModel
            {
                TotalProperties = await _context.Properties.CountAsync(),
                AvailableProperties = await _context.Properties.CountAsync(p => p.Status == "Available"),
                RentedProperties = await _context.Properties.CountAsync(p => p.Status == "Rented"),
                MaintenanceProperties = await _context.Properties.CountAsync(p => p.Status == "Maintenance"),
                
                TotalOwners = await _context.Owners.CountAsync(),
                TotalTenants = await _context.Tenants.CountAsync(),
                ActiveContracts = await _context.Contracts.CountAsync(c => c.IsActive),
                
                TotalPayments = await _context.Payments.CountAsync(),
                PendingPayments = await _context.Payments.CountAsync(p => p.Status == "Pending"),
                PaidPayments = await _context.Payments.CountAsync(p => p.Status == "Paid"),
                OverduePayments = await _context.Payments.CountAsync(p => p.Status == "Overdue"),
                
                TotalMaintenanceRequests = await _context.MaintenanceRequests.CountAsync(),
                PendingMaintenanceRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status == MaintenanceStatus.Submitted),
                InProgressMaintenanceRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status == MaintenanceStatus.InProgress),
                CompletedMaintenanceRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status == MaintenanceStatus.Completed),
                
                MonthlyRevenue = await CalculateMonthlyRevenue(),
                YearlyRevenue = await CalculateYearlyRevenue(),
                
                PropertyTypeDistribution = await GetPropertyTypeDistribution(),
                MonthlyRevenueChart = await GetMonthlyRevenueChart()
            };

            return View(model);
        }

        public async Task<IActionResult> Financial(DateTime? fromDate, DateTime? toDate)
        {
            fromDate ??= DateTime.Now.AddMonths(-12);
            toDate ??= DateTime.Now;

            var payments = await _context.Payments
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Property)
                .Include(p => p.Contract)
                    .ThenInclude(c => c.Tenant)
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var totalRevenue = payments.Where(p => p.Status == "Paid").Sum(p => p.Amount);
            var pendingAmount = payments.Where(p => p.Status == "Pending").Sum(p => p.Amount);
            var overdueAmount = payments.Where(p => p.Status == "Overdue").Sum(p => p.Amount);
            var totalAmount = totalRevenue + pendingAmount + overdueAmount;

            var model = new FinancialReportViewModel
            {
                FromDate = fromDate.Value,
                ToDate = toDate.Value,
                Payments = payments,
                TotalRevenue = totalRevenue,
                PendingAmount = pendingAmount,
                OverdueAmount = overdueAmount,
                CollectionRate = totalAmount > 0 ? (totalRevenue / totalAmount) * 100 : 0
            };

            return View(model);
        }

        public async Task<IActionResult> Properties()
        {
            var properties = await _context.Properties
                .Include(p => p.Owner)
                .Include(p => p.Contracts.Where(c => c.IsActive))
                    .ThenInclude(c => c.Tenant)
                .ToListAsync();

            var totalPotentialRevenue = properties.Sum(p => p.MonthlyRent);
            var actualRevenue = properties.Where(p => p.Status == "Rented").Sum(p => p.MonthlyRent);
            var rentedCount = properties.Count(p => p.Status == "Rented");

            var model = new PropertiesReportViewModel
            {
                Properties = properties,
                TotalProperties = properties.Count,
                AvailableProperties = properties.Count(p => p.Status == "Available"),
                RentedProperties = rentedCount,
                MaintenanceProperties = properties.Count(p => p.Status == "Maintenance"),
                AverageRent = properties.Any() ? properties.Average(p => p.MonthlyRent) : 0,
                TotalPotentialRevenue = totalPotentialRevenue,
                ActualRevenue = actualRevenue,
                OccupancyRate = properties.Count > 0 ? (decimal)rentedCount / properties.Count * 100 : 0,
                RevenueEfficiency = totalPotentialRevenue > 0 ? (actualRevenue / totalPotentialRevenue) * 100 : 0
            };

            return View(model);
        }

        public async Task<IActionResult> Maintenance(DateTime? fromDate, DateTime? toDate)
        {
            fromDate ??= DateTime.Now.AddMonths(-6);
            toDate ??= DateTime.Now;

            var requests = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .Include(m => m.Tenant)
                .Where(m => m.RequestDate >= fromDate && m.RequestDate <= toDate)
                .OrderByDescending(m => m.RequestDate)
                .ToListAsync();

            var model = new MaintenanceReportViewModel
            {
                MaintenanceRequests = requests,
                TotalRequests = requests.Count,
                PendingRequests = requests.Count(m => m.Status == MaintenanceStatus.Submitted),
                InProgressRequests = requests.Count(m => m.Status == MaintenanceStatus.InProgress),
                CompletedRequests = requests.Count(m => m.Status == MaintenanceStatus.Completed),
                CancelledRequests = requests.Count(m => m.Status == MaintenanceStatus.Cancelled),
                TotalCost = requests.Where(m => m.ActualCost.HasValue).Sum(m => m.ActualCost!.Value),
                AverageCompletionTime = CalculateAverageCompletionTime(requests),
                AverageCost = requests.Any() && requests.Where(m => m.ActualCost.HasValue).Any() ? requests.Where(m => m.ActualCost.HasValue).Average(m => m.ActualCost!.Value) : 0,
                RequestsThisMonth = await _context.MaintenanceRequests.CountAsync(m => m.RequestDate.Month == DateTime.Now.Month && m.RequestDate.Year == DateTime.Now.Year),
                CategoryDistribution = await GetCategoryDistribution()
            };

            model.CompletionRate = model.TotalRequests > 0 ? (decimal)model.CompletedRequests / model.TotalRequests * 100 : 0;

            return View(model);
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

        private async Task<decimal> CalculateYearlyRevenue()
        {
            var currentYear = DateTime.Now.Year;

            return await _context.Payments
                .Where(p => p.Status == "Paid" &&
                           p.PaymentDate.HasValue &&
                           p.PaymentDate.Value.Year == currentYear)
                .SumAsync(p => p.Amount);
        }

        private async Task<Dictionary<string, int>> GetPropertyTypeDistribution()
        {
            return await _context.Properties
                .GroupBy(p => p.PropertyType)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        private async Task<Dictionary<string, decimal>> GetMonthlyRevenueChart()
        {
            var result = new Dictionary<string, decimal>();
            var currentDate = DateTime.Now;

            for (int i = 11; i >= 0; i--)
            {
                var month = currentDate.AddMonths(-i);
                var revenue = await _context.Payments
                    .Where(p => p.Status == "Paid" &&
                               p.PaymentDate.HasValue &&
                               p.PaymentDate.Value.Month == month.Month &&
                               p.PaymentDate.Value.Year == month.Year)
                    .SumAsync(p => p.Amount);

                result.Add(month.ToString("MMM yyyy"), revenue);
            }

            return result;
        }

        private double CalculateAverageCompletionTime(List<MaintenanceRequest> requests)
        {
            var completedRequests = requests
                .Where(m => m.Status == MaintenanceStatus.Completed && m.CompletedDate.HasValue)
                .ToList();

            if (!completedRequests.Any())
                return 0;

            var totalDays = completedRequests
                .Sum(m => (m.CompletedDate!.Value - m.RequestDate).TotalDays);

            return totalDays / completedRequests.Count;
        }

        private async Task<List<int>> GetCategoryDistribution()
        {
            var distribution = new List<int>();

            for (int i = 1; i <= 11; i++) // Categories 1-11
            {
                var count = await _context.MaintenanceRequests.CountAsync(m => m.Category == (MaintenanceCategory)i);
                distribution.Add(count);
            }

            return distribution;
        }
    }
}

using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class ReportsViewModel
    {
        // Property Statistics
        public int TotalProperties { get; set; }
        public int AvailableProperties { get; set; }
        public int RentedProperties { get; set; }
        public int MaintenanceProperties { get; set; }

        // People Statistics
        public int TotalOwners { get; set; }
        public int TotalTenants { get; set; }
        public int ActiveContracts { get; set; }

        // Payment Statistics
        public int TotalPayments { get; set; }
        public int PendingPayments { get; set; }
        public int PaidPayments { get; set; }
        public int OverduePayments { get; set; }

        // Maintenance Statistics
        public int TotalMaintenanceRequests { get; set; }
        public int PendingMaintenanceRequests { get; set; }
        public int InProgressMaintenanceRequests { get; set; }
        public int CompletedMaintenanceRequests { get; set; }

        // Financial Statistics
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }

        // Charts Data
        public Dictionary<string, int> PropertyTypeDistribution { get; set; } = new();
        public Dictionary<string, decimal> MonthlyRevenueChart { get; set; } = new();

        // Calculated Properties
        public decimal OccupancyRate => TotalProperties > 0 ? (decimal)RentedProperties / TotalProperties * 100 : 0;
        public decimal MaintenanceRate => TotalProperties > 0 ? (decimal)MaintenanceProperties / TotalProperties * 100 : 0;
        public decimal PaymentSuccessRate => TotalPayments > 0 ? (decimal)PaidPayments / TotalPayments * 100 : 0;
    }

    public class FinancialReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<Payment> Payments { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal CollectionRate { get; set; }
    }

    public class PropertiesReportViewModel
    {
        public List<Property> Properties { get; set; } = new();
        public int TotalProperties { get; set; }
        public int AvailableProperties { get; set; }
        public int RentedProperties { get; set; }
        public int MaintenanceProperties { get; set; }
        public decimal AverageRent { get; set; }
        public decimal TotalPotentialRevenue { get; set; }
        public decimal ActualRevenue { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal RevenueEfficiency { get; set; }
    }

    public class MaintenanceReportViewModel
    {
        public List<MaintenanceRequest> MaintenanceRequests { get; set; } = new();
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int CancelledRequests { get; set; }
        public decimal TotalCost { get; set; }
        public double AverageCompletionTime { get; set; }
        public decimal AverageCost { get; set; }
        public int RequestsThisMonth { get; set; }
        public decimal CompletionRate { get; set; }
        public List<int> CategoryDistribution { get; set; } = new();
    }
}

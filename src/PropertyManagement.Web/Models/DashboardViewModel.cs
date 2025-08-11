using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalProperties { get; set; }
        public int AvailableProperties { get; set; }
        public int RentedProperties { get; set; }
        public int ActiveContracts { get; set; }
        public int PendingPayments { get; set; }
        public int OverduePayments { get; set; }
        public int MaintenanceRequests { get; set; }
        public decimal MonthlyRevenue { get; set; }
        
        public List<Property> RecentProperties { get; set; } = new();
        public List<Payment> RecentPayments { get; set; } = new();
        
        public decimal OccupancyRate { get; set; }
    }
}

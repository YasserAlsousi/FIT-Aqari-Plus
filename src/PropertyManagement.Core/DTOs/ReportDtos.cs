namespace PropertyManagement.Core.DTOs
{
    public class FinancialReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal OutstandingPayments { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public List<PaymentTypeBredownDto> PaymentBreakdown { get; set; } = new();
    }
    
    public class MonthlyRevenueDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
    }
    
    public class PaymentTypeBredownDto
    {
        public string PaymentType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }
    
    public class PropertyPerformanceReportDto
    {
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal OccupancyRate { get; set; }
        public int TotalContracts { get; set; }
        public decimal AverageRent { get; set; }
        public List<MonthlyPerformanceDto> MonthlyPerformance { get; set; } = new();
    }
    
    public class MonthlyPerformanceDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public bool IsOccupied { get; set; }
    }
    
    public class OccupancyReportDto
    {
        public int TotalProperties { get; set; }
        public int OccupiedProperties { get; set; }
        public int AvailableProperties { get; set; }
        public decimal OccupancyRate { get; set; }
        public List<PropertyTypeOccupancyDto> ByPropertyType { get; set; } = new();
    }
    
    public class PropertyTypeOccupancyDto
    {
        public string PropertyType { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Occupied { get; set; }
        public decimal OccupancyRate { get; set; }
    }
    
    public class MaintenanceReportDto
    {
        public int TotalRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int PendingRequests { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageCost { get; set; }
        public List<MaintenanceCategoryDto> ByCategory { get; set; } = new();
    }
    
    public class MaintenanceCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalCost { get; set; }
    }
}
using PropertyManagement.Core.DTOs;

namespace PropertyManagement.Core.Services
{
    public interface IReportService
    {
        Task<FinancialReportDto> GetFinancialReportAsync(DateTime fromDate, DateTime toDate);
        Task<PropertyPerformanceReportDto> GetPropertyPerformanceReportAsync(int propertyId, DateTime fromDate, DateTime toDate);
        Task<OccupancyReportDto> GetOccupancyReportAsync();
        Task<MaintenanceReportDto> GetMaintenanceReportAsync(DateTime fromDate, DateTime toDate);
    }
}
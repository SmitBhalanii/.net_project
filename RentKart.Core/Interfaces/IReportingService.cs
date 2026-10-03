using System;
using System.Threading.Tasks;
using RentKart.Core.DTOs.Reports;

namespace RentKart.Core.Interfaces
{
    public interface IReportingService
    {
        Task<AdminDashboardDto> GetAdminDashboardAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<BusinessDashboardDto> GetBusinessDashboardAsync(int businessId, DateTime? startDate = null, DateTime? endDate = null);
        Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(string customerId);
    }
}

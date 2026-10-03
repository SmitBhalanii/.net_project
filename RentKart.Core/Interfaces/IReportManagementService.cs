using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;
using RentKart.Core.Enums;

namespace RentKart.Core.Interfaces;

public interface IReportManagementService
{
    Task<IEnumerable<Report>> GetAllReportsAsync();
    Task<Report?> GetReportByIdAsync(int id);
    Task CreateReportAsync(Report report);
    Task UpdateReportStatusAsync(int id, ReportStatus status, string resolvedByUserId, string? resolutionNotes);
}

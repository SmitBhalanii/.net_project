using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;
using RentKart.Infrastructure.Data;

namespace RentKart.Infrastructure.Services;

public class ReportManagementService : IReportManagementService
{
    private readonly ApplicationDbContext _db;

    public ReportManagementService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Report>> GetAllReportsAsync()
    {
        return await _db.Reports
            .Include(r => r.Reporter)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Report?> GetReportByIdAsync(int id)
    {
        return await _db.Reports
            .Include(r => r.Reporter)
            .Include(r => r.ResolvedBy)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task CreateReportAsync(Report report)
    {
        _db.Reports.Add(report);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateReportStatusAsync(int id, ReportStatus status, string resolvedByUserId, string? resolutionNotes)
    {
        var report = await _db.Reports.FindAsync(id);
        if (report != null)
        {
            report.Status = status;
            if (status == ReportStatus.Resolved || status == ReportStatus.Rejected)
            {
                report.ResolvedAt = DateTime.UtcNow;
                report.ResolvedByUserId = resolvedByUserId;
                report.ResolutionNotes = resolutionNotes;
            }
            _db.Reports.Update(report);
            await _db.SaveChangesAsync();
        }
    }
}

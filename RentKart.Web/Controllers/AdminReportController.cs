using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminReportController : Controller
{
    private readonly IReportManagementService _reportService;
    private readonly IAuditLogService _auditLogService;

    public AdminReportController(IReportManagementService reportService, IAuditLogService auditLogService)
    {
        _reportService = reportService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var reports = await _reportService.GetAllReportsAsync();
        return View(reports);
    }

    public async Task<IActionResult> Details(int id)
    {
        var report = await _reportService.GetReportByIdAsync(id);
        if (report == null) return NotFound();
        return View(report);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ReportStatus status, string? resolutionNotes)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        await _reportService.UpdateReportStatusAsync(id, status, userId, resolutionNotes);
        await _auditLogService.LogActionAsync(userId, "UpdateReportStatus", "Report", id.ToString(), $"Report resolved with status {status}");
        
        TempData["SuccessMessage"] = $"Report status updated to {status}.";
        return RedirectToAction(nameof(Index));
    }
}

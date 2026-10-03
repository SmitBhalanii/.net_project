using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminBusinessController : Controller
{
    private readonly IBusinessService _businessService;
    private readonly IAuditLogService _auditLogService;

    public AdminBusinessController(IBusinessService businessService, IAuditLogService auditLogService)
    {
        _businessService = businessService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var businesses = await _businessService.GetAllBusinessesAsync();
        return View(businesses);
    }

    public async Task<IActionResult> Details(int id)
    {
        var business = await _businessService.GetBusinessByIdAsync(id);
        if (business == null) return NotFound();
        return View(business);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, BusinessApprovalStatus status, string? reason)
    {
        await _businessService.UpdateBusinessStatusAsync(id, status);
        await _auditLogService.LogActionAsync(User.Identity?.Name, "UpdateBusinessStatus", "Business", id.ToString(), $"Status changed to {status}. Reason: {reason}");
        TempData["SuccessMessage"] = $"Business status updated to {status}.";
        return RedirectToAction(nameof(Index));
    }
}

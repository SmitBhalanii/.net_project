using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminEquipmentController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly IAuditLogService _auditLogService;

    public AdminEquipmentController(IEquipmentService equipmentService, IAuditLogService auditLogService)
    {
        _equipmentService = equipmentService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var equipment = await _equipmentService.GetRecentEquipmentAsync(100);
        return View(equipment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, bool isActive, string? reason)
    {
        var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
        if (equipment == null) return NotFound();

        equipment.IsActive = isActive;
        await _equipmentService.UpdateEquipmentAsync(equipment);

        string statusText = isActive ? "Reactivated" : "Deactivated";
        await _auditLogService.LogActionAsync(User.Identity?.Name, "UpdateEquipmentStatus", "Equipment", id.ToString(), $"Status changed to {statusText}. Reason: {reason}");

        TempData["SuccessMessage"] = $"Equipment {statusText}.";
        return RedirectToAction(nameof(Index));
    }
}

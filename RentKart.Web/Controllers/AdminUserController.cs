using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminUserController : Controller
{
    private readonly IUserManagementService _userService;
    private readonly IAuditLogService _auditLogService;

    public AdminUserController(IUserManagementService userService, IAuditLogService auditLogService)
    {
        _userService = userService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllUsersAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string id, bool isActive, string? reason)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        await _userService.UpdateUserStatusAsync(id, isActive);
        
        string statusText = isActive ? "Reactivated" : "Suspended";
        await _auditLogService.LogActionAsync(User.Identity?.Name, "UpdateUserStatus", "ApplicationUser", id, $"Status changed to {statusText}. Reason: {reason}");
        
        TempData["SuccessMessage"] = $"User account {statusText}.";
        return RedirectToAction(nameof(Index));
    }
}

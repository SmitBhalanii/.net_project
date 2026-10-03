using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminSettingsController : Controller
{
    private readonly IPlatformSettingsService _settingsService;
    private readonly IAuditLogService _auditLogService;

    public AdminSettingsController(IPlatformSettingsService settingsService, IAuditLogService auditLogService)
    {
        _settingsService = settingsService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.MarketplaceActive = await _settingsService.GetSettingValueAsync("MarketplaceActive") ?? "True";
        ViewBag.PlatformName = await _settingsService.GetSettingValueAsync("PlatformName") ?? "RentKart";
        ViewBag.SupportEmail = await _settingsService.GetSettingValueAsync("SupportEmail") ?? "support@rentkart.com";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string key, string value)
    {
        await _settingsService.SetSettingValueAsync(key, value);
        await _auditLogService.LogActionAsync(User.Identity?.Name, "UpdateSetting", "PlatformSetting", key, $"Setting {key} changed to {value}");
        
        TempData["SuccessMessage"] = "Platform settings updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}

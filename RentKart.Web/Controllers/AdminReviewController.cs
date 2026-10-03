using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminReviewController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly IAuditLogService _auditLogService;

    public AdminReviewController(IReviewService reviewService, IAuditLogService auditLogService)
    {
        _reviewService = reviewService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var reviews = await _reviewService.GetPendingReviewsAsync();
        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ReviewStatus status)
    {
        await _reviewService.ModerateReviewAsync(id, status);
        await _auditLogService.LogActionAsync(User.Identity?.Name, "UpdateReviewStatus", "Review", id.ToString(), $"Status changed to {status}.");
        
        TempData["SuccessMessage"] = $"Review status updated to {status}.";
        return RedirectToAction(nameof(Index));
    }
}

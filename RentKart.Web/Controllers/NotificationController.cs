using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;
using RentKart.Web.ViewModels.Notification;

namespace RentKart.Web.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index(int page = 1, bool unreadOnly = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, 20, unreadOnly);
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

        var viewModel = new NotificationListViewModel
        {
            UnreadCount = unreadCount,
            Notifications = notifications.Select(n => new NotificationViewModel
            {
                Id = n.Id,
                NotificationType = n.NotificationType,
                Title = n.Title,
                Message = n.Message,
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
        };

        ViewData["UnreadOnly"] = unreadOnly;
        ViewData["CurrentPage"] = page;
        
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        await _notificationService.MarkAsReadAsync(userId, id);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        await _notificationService.MarkAllAsReadAsync(userId);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        await _notificationService.DeleteAsync(userId, id);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Preferences()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var prefs = await _notificationService.GetPreferencesAsync(userId);
        return View(prefs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preferences(NotificationPreference model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        model.UserId = userId;
        await _notificationService.UpdatePreferencesAsync(model);

        TempData["SuccessMessage"] = "Notification preferences updated successfully.";
        return RedirectToAction(nameof(Preferences));
    }
}

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminDashboardController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly IReportingService _reportingService;

    public AdminDashboardController(INotificationService notificationService, IReportingService reportingService)
    {
        _notificationService = notificationService;
        _reportingService = reportingService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, 1, 5);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            
            ViewBag.RecentNotifications = notifications.Select(n => new RentKart.Web.ViewModels.Notification.NotificationViewModel
            {
                Id = n.Id,
                NotificationType = n.NotificationType,
                Title = n.Title,
                Message = n.Message,
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
            ViewBag.UnreadNotificationCount = unreadCount;
        }

        var report = await _reportingService.GetAdminDashboardAsync();
        return View(report);
    }
}

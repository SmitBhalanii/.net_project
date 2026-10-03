using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Interfaces;
using RentKart.Web.ViewModels.Notification;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Customer)]
public class CustomerDashboardController : Controller
{
    private readonly IRentalService _rentalService;
    private readonly INotificationService _notificationService;

    public CustomerDashboardController(IRentalService rentalService, INotificationService notificationService)
    {
        _rentalService = rentalService;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            await _rentalService.ProcessDueNotificationsAsync();
            
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, 1, 5);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            
            ViewBag.RecentNotifications = notifications.Select(n => new NotificationViewModel
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

        return View();
    }
}

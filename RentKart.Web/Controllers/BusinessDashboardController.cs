using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Business)]
public class BusinessDashboardController : Controller
{
    private readonly IRentalService _rentalService;
    private readonly IBusinessService _businessService;
    private readonly Microsoft.AspNetCore.Identity.UserManager<RentKart.Core.Entities.ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;

    public BusinessDashboardController(IRentalService rentalService, IBusinessService businessService, Microsoft.AspNetCore.Identity.UserManager<RentKart.Core.Entities.ApplicationUser> userManager, INotificationService notificationService)
    {
        _rentalService = rentalService;
        _businessService = businessService;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    public async System.Threading.Tasks.Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var business = await _businessService.GetBusinessByUserIdAsync(user.Id);
        if (business == null) return View(); // or redirect

        var rentals = await _rentalService.GetBusinessRentalsAsync(business.Id);

        ViewBag.ReadyForPickup = rentals.Count(r => r.Status == RentKart.Core.Enums.RentalStatus.ReadyForPickup);
        ViewBag.ActiveRentals = rentals.Count(r => r.Status == RentKart.Core.Enums.RentalStatus.Active);
        ViewBag.TodayReturns = rentals.Count(r => r.Status == RentKart.Core.Enums.RentalStatus.Returned && r.ActualReturnDate?.Date == System.DateTime.UtcNow.Date);
        ViewBag.OverdueRentals = rentals.Count(r => r.Status == RentKart.Core.Enums.RentalStatus.Active && System.DateTime.UtcNow.Date > r.ExpectedReturnDate.Date);

        await _rentalService.ProcessDueNotificationsAsync();

        var notifications = await _notificationService.GetUserNotificationsAsync(user.Id, 1, 5);
        var unreadCount = await _notificationService.GetUnreadCountAsync(user.Id);

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

        return View();
    }
}

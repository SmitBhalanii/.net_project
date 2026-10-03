using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Constants;
using RentKart.Core.Interfaces;
using RentKart.Infrastructure.Data;
using RentKart.Web.Models.ViewModels;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Business)]
public class BusinessDashboardController : Controller
{
    private readonly IRentalService _rentalService;
    private readonly IBusinessService _businessService;
    private readonly Microsoft.AspNetCore.Identity.UserManager<RentKart.Core.Entities.ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly ApplicationDbContext _context;

    public BusinessDashboardController(
        IRentalService rentalService, 
        IBusinessService businessService, 
        Microsoft.AspNetCore.Identity.UserManager<RentKart.Core.Entities.ApplicationUser> userManager, 
        INotificationService notificationService,
        ApplicationDbContext context)
    {
        _rentalService = rentalService;
        _businessService = businessService;
        _userManager = userManager;
        _notificationService = notificationService;
        _context = context;
    }

    public async System.Threading.Tasks.Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var business = await _businessService.GetBusinessByUserIdAsync(user.Id);
        if (business == null) return View(); // or redirect

        // Dashboard KPIs via server-side aggregation
        var equipmentStats = await _context.Equipment
            .Where(e => e.BusinessId == business.Id)
            .GroupBy(e => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(e => e.IsActive && e.Status == RentKart.Core.Enums.EquipmentStatus.Active)
            })
            .FirstOrDefaultAsync();

        var pendingBookings = await _context.Bookings
            .Where(b => b.BusinessId == business.Id && b.Status == RentKart.Core.Enums.BookingStatus.Pending)
            .CountAsync();

        var rentals = await _rentalService.GetBusinessRentalsAsync(business.Id);

        var viewModel = new VendorDashboardViewModel
        {
            TotalEquipment = equipmentStats?.Total ?? 0,
            ActiveEquipment = equipmentStats?.Active ?? 0,
            PendingBookings = pendingBookings,
            ReadyForPickup = rentals.Count(r => r.Status == RentKart.Core.Enums.RentalStatus.ReadyForPickup),
            ActiveRentals = rentals.Count(r => r.Status == RentKart.Core.Enums.RentalStatus.Active),
            TodayReturns = rentals.Count(r => r.Status == RentKart.Core.Enums.RentalStatus.Returned && r.ActualReturnDate?.Date == System.DateTime.UtcNow.Date),
            OverdueRentals = rentals.Count(r => r.Status == RentKart.Core.Enums.RentalStatus.Active && System.DateTime.UtcNow.Date > r.ExpectedReturnDate.Date)
        };

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

        return View(viewModel);
    }
}

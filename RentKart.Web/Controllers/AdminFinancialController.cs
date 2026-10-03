using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentKart.Infrastructure.Data;
using RentKart.Core.Constants;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminFinancialController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminFinancialController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalPayments = await _context.Payments
            .Where(p => p.PaymentStatus == RentKart.Core.Enums.PaymentStatus.Succeeded)
            .SumAsync(p => p.Amount);
            
        var totalRefunds = await _context.Refunds.SumAsync(r => r.Amount);
        
        var completedBookings = await _context.Bookings
            .Where(b => b.PaymentStatus == RentKart.Core.Enums.PaymentStatus.Succeeded)
            .ToListAsync();
            
        var totalRentalRevenue = completedBookings.Sum(b => b.RentalAmount);
        var totalDepositsHeld = completedBookings.Sum(b => b.SecurityDepositAmount);

        ViewBag.TotalPayments = totalPayments;
        ViewBag.TotalRefunds = totalRefunds;
        ViewBag.TotalRentalRevenue = totalRentalRevenue;
        ViewBag.TotalDepositsHeld = totalDepositsHeld;

        var recentTransactions = await _context.Payments
            .Include(p => p.Booking)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .ToListAsync();
            
        ViewBag.RecentTransactions = recentTransactions;

        return View();
    }
}

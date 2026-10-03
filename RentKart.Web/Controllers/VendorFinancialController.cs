using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Infrastructure.Data;
using RentKart.Core.Constants;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Business)]
public class VendorFinancialController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBusinessService _businessService;
    private readonly IRefundService _refundService;

    public VendorFinancialController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IBusinessService businessService, IRefundService refundService)
    {
        _context = context;
        _userManager = userManager;
        _businessService = businessService;
        _refundService = refundService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var business = await _businessService.GetBusinessByUserIdAsync(user.Id);
        if (business == null) return Forbid();

        var payments = await _context.Payments
            .Include(p => p.Booking)
            .Where(p => p.Booking.BusinessId == business.Id && p.PaymentStatus == RentKart.Core.Enums.PaymentStatus.Succeeded || p.PaymentStatus == RentKart.Core.Enums.PaymentStatus.PartiallyRefunded)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
            
        var refunds = await _context.Refunds
            .Include(r => r.Booking)
            .Where(r => r.Booking.BusinessId == business.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
            
        var completedBookings = await _context.Bookings
            .Where(b => b.BusinessId == business.Id && (b.PaymentStatus == RentKart.Core.Enums.PaymentStatus.Succeeded || b.PaymentStatus == RentKart.Core.Enums.PaymentStatus.PartiallyRefunded))
            .ToListAsync();
            
        var actualRevenue = completedBookings.Sum(b => b.RentalAmount);

        ViewBag.TotalRevenue = actualRevenue;
        ViewBag.TotalPayments = payments.Sum(p => p.Amount);
        ViewBag.TotalRefunds = refunds.Sum(r => r.Amount);
        
        ViewBag.Payments = payments.Take(20).ToList();
        ViewBag.Refunds = refunds.Take(20).ToList();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ProcessRefund(int paymentId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var business = await _businessService.GetBusinessByUserIdAsync(user.Id);
        if (business == null) return Forbid();

        var payment = await _context.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null || payment.Booking.BusinessId != business.Id) return NotFound();

        var totalRefunded = await _context.Refunds
            .Where(r => r.PaymentId == paymentId)
            .SumAsync(r => r.Amount);

        ViewBag.MaxRefund = payment.Amount - totalRefunded;

        return View(payment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessRefund(int paymentId, decimal amount, string reason)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var business = await _businessService.GetBusinessByUserIdAsync(user.Id);
        if (business == null) return Forbid();

        try
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null || payment.Booking.BusinessId != business.Id) return NotFound();

            await _refundService.ProcessRefundAsync(paymentId, payment.CustomerId, amount, reason, user.Id);
            
            TempData["SuccessMessage"] = "Refund processed successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (System.Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(ProcessRefund), new { paymentId });
        }
    }
}

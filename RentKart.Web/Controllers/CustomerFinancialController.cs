using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Infrastructure.Data;
using RentKart.Core.Constants;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Customer)]
public class CustomerFinancialController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerFinancialController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var payments = await _context.Payments
            .Include(p => p.Booking)
            .Where(p => p.CustomerId == user.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        var invoices = await _context.Invoices
            .Include(i => i.Booking)
            .Where(i => i.Booking.CustomerId == user.Id)
            .OrderByDescending(i => i.IssuedAt)
            .Take(10)
            .ToListAsync();

        var refunds = await _context.Refunds
            .Include(r => r.Booking)
            .Where(r => r.CustomerId == user.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .ToListAsync();

        ViewBag.Payments = payments;
        ViewBag.Invoices = invoices;
        ViewBag.Refunds = refunds;

        return View();
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Infrastructure.Data;

namespace RentKart.Web.Controllers;

[Authorize]
public class InvoiceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InvoiceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();
        
        var isVendor = await _userManager.IsInRoleAsync(user, "Vendor");
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        var invoice = await _context.Invoices
            .Include(i => i.Booking)
                .ThenInclude(b => b.Equipment)
            .Include(i => i.Booking)
                .ThenInclude(b => b.Customer)
            .Include(i => i.Booking)
                .ThenInclude(b => b.Business)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return NotFound();

        // Security check
        if (!isAdmin && invoice.Booking.CustomerId != user.Id && (!isVendor || invoice.Booking.Business.UserId != user.Id))
        {
            return Forbid();
        }

        return View(invoice);
    }
    
    // In a real app we might generate PDF here
    public async Task<IActionResult> Download(int id)
    {
        return RedirectToAction("Details", new { id });
    }
}

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Interfaces;
using RentKart.Web.ViewModels.Booking;

namespace RentKart.Web.Controllers;

[Authorize(Roles = "Business")]
public class BusinessBookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IBusinessService _businessService;

    public BusinessBookingController(IBookingService bookingService, IBusinessService businessService)
    {
        _bookingService = bookingService;
        _businessService = businessService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var business = await _businessService.GetBusinessByUserIdAsync(userId);
        if (business == null) return NotFound("Business profile not found.");

        var bookings = await _bookingService.GetBusinessBookingsAsync(business.Id);

        var viewModels = bookings.Select(b => new BusinessBookingViewModel
        {
            Id = b.Id,
            BookingNumber = b.BookingNumber,
            CustomerName = b.Customer.DisplayName ?? b.Customer.FirstName,
            EquipmentName = b.Equipment.Name,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            RentalAmount = b.RentalAmount,
            SecurityDeposit = b.SecurityDepositAmount,
            TotalAmount = b.TotalAmount,
            Status = b.Status,
            CustomerNote = b.CustomerNote
        });

        return View(viewModels);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? businessNote)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var business = await _businessService.GetBusinessByUserIdAsync(userId);
        if (business == null) return NotFound();

        try
        {
            var success = await _bookingService.ApproveBookingAsync(id, business.Id, businessNote);
            if (success)
            {
                TempData["SuccessMessage"] = "Booking approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not approve the booking. It may not exist or you don't have permission.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? businessNote)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var business = await _businessService.GetBusinessByUserIdAsync(userId);
        if (business == null) return NotFound();

        try
        {
            var success = await _bookingService.RejectBookingAsync(id, business.Id, businessNote);
            if (success)
            {
                TempData["SuccessMessage"] = "Booking rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not reject the booking.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}

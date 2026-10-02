using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;
using RentKart.Web.ViewModels.Booking;

namespace RentKart.Web.Controllers;

[Authorize(Roles = "Customer")]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IEquipmentService _equipmentService;

    public BookingController(IBookingService bookingService, IEquipmentService equipmentService)
    {
        _bookingService = bookingService;
        _equipmentService = equipmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int equipmentId)
    {
        var equipment = await _equipmentService.GetEquipmentByIdAsync(equipmentId);
        if (equipment == null || !equipment.IsActive)
        {
            return NotFound("Equipment not found or inactive.");
        }
        
        // Also ensure business is approved, etc. Service handles that on POST, but good to check here too
        if (equipment.Business.ApprovalStatus != Core.Enums.BusinessApprovalStatus.Approved)
        {
            return NotFound("Equipment provider is not currently active.");
        }

        var viewModel = new BookingCreateViewModel
        {
            EquipmentId = equipment.Id,
            EquipmentName = equipment.Name,
            BusinessName = equipment.Business.BusinessName,
            DailyRate = equipment.RentalPrice,
            SecurityDeposit = equipment.SecurityDeposit,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        if (model.StartDate < DateTime.Today)
        {
            ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
        }

        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError("EndDate", "End date cannot be before start date.");
        }

        if (!ModelState.IsValid)
        {
            // Reload equipment info if invalid
            var equipment = await _equipmentService.GetEquipmentByIdAsync(model.EquipmentId);
            if (equipment != null)
            {
                model.EquipmentName = equipment.Name;
                model.BusinessName = equipment.Business.BusinessName;
                model.DailyRate = equipment.RentalPrice;
                model.SecurityDeposit = equipment.SecurityDeposit;
            }
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var booking = new Booking
            {
                EquipmentId = model.EquipmentId,
                CustomerId = userId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CustomerNote = model.CustomerNote
            };

            var createdBooking = await _bookingService.CreateBookingAsync(booking);
            
            TempData["SuccessMessage"] = "Booking request submitted successfully!";
            return RedirectToAction(nameof(Confirmation), new { bookingNumber = createdBooking.BookingNumber });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            
            // Reload info
            var equipment = await _equipmentService.GetEquipmentByIdAsync(model.EquipmentId);
            if (equipment != null)
            {
                model.EquipmentName = equipment.Name;
                model.BusinessName = equipment.Business.BusinessName;
                model.DailyRate = equipment.RentalPrice;
                model.SecurityDeposit = equipment.SecurityDeposit;
            }
            
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(string bookingNumber)
    {
        var booking = await _bookingService.GetBookingByNumberAsync(bookingNumber);
        if (booking == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (booking.CustomerId != userId) return Forbid();

        var viewModel = new BookingDetailsViewModel
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            EquipmentName = booking.Equipment.Name,
            BusinessName = booking.Business.BusinessName,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            RentalDays = (booking.EndDate - booking.StartDate).Days + 1,
            DailyRate = booking.DailyRate,
            RentalAmount = booking.RentalAmount,
            SecurityDeposit = booking.SecurityDepositAmount,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            CustomerNote = booking.CustomerNote,
            CreatedAt = booking.CreatedAt
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var bookings = await _bookingService.GetCustomerBookingsAsync(userId);
        
        var viewModels = bookings.Select(b => new BookingListViewModel
        {
            Id = b.Id,
            BookingNumber = b.BookingNumber,
            EquipmentName = b.Equipment.Name,
            BusinessName = b.Business.BusinessName,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            TotalAmount = b.TotalAmount,
            Status = b.Status
        });

        return View(viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (booking.CustomerId != userId) return Forbid();

        var viewModel = new BookingDetailsViewModel
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            EquipmentName = booking.Equipment.Name,
            BusinessName = booking.Business.BusinessName,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            RentalDays = (booking.EndDate - booking.StartDate).Days + 1,
            DailyRate = booking.DailyRate,
            RentalAmount = booking.RentalAmount,
            SecurityDeposit = booking.SecurityDepositAmount,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            CustomerNote = booking.CustomerNote,
            BusinessNote = booking.BusinessNote,
            CreatedAt = booking.CreatedAt
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var success = await _bookingService.CancelBookingAsync(id, userId, null);
            if (success)
            {
                TempData["SuccessMessage"] = "Booking cancelled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not cancel the booking.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;
using RentKart.Web.ViewModels.Booking;

using Microsoft.AspNetCore.Identity;

namespace RentKart.Web.Controllers;

[Authorize(Roles = "Customer")]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IEquipmentService _equipmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingController(
        IBookingService bookingService, 
        IEquipmentService equipmentService,
        UserManager<ApplicationUser> userManager)
    {
        _bookingService = bookingService;
        _equipmentService = equipmentService;
        _userManager = userManager;
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

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        var equipmentObj = await _equipmentService.GetEquipmentByIdAsync(model.EquipmentId);
        if (equipmentObj == null || !equipmentObj.IsActive || equipmentObj.Business.ApprovalStatus != Core.Enums.BusinessApprovalStatus.Approved)
        {
            return NotFound("Equipment not found or unavailable.");
        }

        try
        {
            var isAvailable = await _bookingService.IsEquipmentAvailableAsync(model.EquipmentId, model.StartDate, model.EndDate);
            if (!isAvailable)
            {
                ModelState.AddModelError(string.Empty, "Equipment is not available for the selected dates.");
                model.EquipmentName = equipmentObj.Name;
                model.BusinessName = equipmentObj.Business.BusinessName;
                model.DailyRate = equipmentObj.RentalPrice;
                model.SecurityDeposit = equipmentObj.SecurityDeposit;
                return View(model);
            }

            int rentalDays = (model.EndDate - model.StartDate).Days + 1;
            decimal rentalAmount = rentalDays * equipmentObj.RentalPrice;

            var summary = new BookingSummaryViewModel
            {
                EquipmentId = equipmentObj.Id,
                EquipmentName = equipmentObj.Name,
                EquipmentImageUrl = equipmentObj.EquipmentImages?.FirstOrDefault()?.ImagePath ?? "",
                BusinessName = equipmentObj.Business.BusinessName,
                PickupLocation = $"{equipmentObj.Business.Address}, {equipmentObj.Business.City}",
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                RentalDays = rentalDays,
                DailyRate = equipmentObj.RentalPrice,
                RentalAmount = rentalAmount,
                SecurityDeposit = equipmentObj.SecurityDeposit,
                TotalAmount = rentalAmount + equipmentObj.SecurityDeposit,
                CustomerName = $"{user.FirstName} {user.LastName}",
                CustomerEmail = user.Email ?? "",
                CustomerPhone = user.PhoneNumber ?? "",
                CustomerNote = model.CustomerNote ?? "",
                TermsAccepted = false
            };

            return View("Summary", summary);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.EquipmentName = equipmentObj.Name;
            model.BusinessName = equipmentObj.Business.BusinessName;
            model.DailyRate = equipmentObj.RentalPrice;
            model.SecurityDeposit = equipmentObj.SecurityDeposit;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(BookingSummaryViewModel model)
    {
        if (!ModelState.IsValid || !model.TermsAccepted)
        {
            if (!model.TermsAccepted)
            {
                ModelState.AddModelError("TermsAccepted", "You must accept the rental terms.");
            }
            return View("Summary", model);
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
            return View("Summary", model);
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
            PaymentStatus = booking.PaymentStatus,
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
            Status = b.Status,
            PaymentStatus = b.PaymentStatus
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
            PaymentStatus = booking.PaymentStatus,
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

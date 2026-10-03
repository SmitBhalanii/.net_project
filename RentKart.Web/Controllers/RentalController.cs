using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Interfaces;
using RentKart.Web.ViewModels.Rental;

namespace RentKart.Web.Controllers;

[Authorize]
public class RentalController : Controller
{
    private readonly IRentalService _rentalService;
    private readonly IBusinessService _businessService;

    public RentalController(IRentalService rentalService, IBusinessService businessService)
    {
        _rentalService = rentalService;
        _businessService = businessService;
    }

    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MyRentals()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var rentals = await _rentalService.GetCustomerRentalsAsync(userId);
        return View(rentals);
    }

    [Authorize(Roles = "Business,VendorStaff")]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var business = await _businessService.GetBusinessByUserIdAsync(userId);
        if (business == null) return NotFound("Business not found");

        var rentals = await _rentalService.GetBusinessRentalsAsync(business.Id);
        return View(rentals);
    }

    public async Task<IActionResult> Details(int id)
    {
        var rental = await _rentalService.GetRentalByIdAsync(id);
        if (rental == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (User.IsInRole("Customer") && rental.CustomerId != userId) return Forbid();
        
        if (User.IsInRole("Business") || User.IsInRole("VendorStaff"))
        {
            var business = await _businessService.GetBusinessByUserIdAsync(userId!);
            if (business == null || rental.BusinessId != business.Id) return Forbid();
        }

        return View(rental);
    }

    [Authorize(Roles = "Business,VendorStaff")]
    public async Task<IActionResult> Issue(int id)
    {
        var rental = await _rentalService.GetRentalByIdAsync(id);
        if (rental == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var business = await _businessService.GetBusinessByUserIdAsync(userId!);
        if (business == null || rental.BusinessId != business.Id) return Forbid();

        var model = new RentalIssueViewModel
        {
            RentalId = rental.Id,
            BookingNumber = rental.Booking.BookingNumber,
            CustomerName = rental.Customer.Email!, // Adjust if Name is available
            EquipmentName = rental.Equipment.Name,
            EquipmentCode = rental.Equipment.EquipmentCode,
            RentalStart = rental.Booking.StartDate,
            RentalEnd = rental.Booking.EndDate,
            PaymentStatus = rental.Booking.PaymentStatus,
            ConditionAtIssue = rental.Equipment.Condition
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Business,VendorStaff")]
    public async Task<IActionResult> Issue(RentalIssueViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            var rental = await _rentalService.GetRentalByIdAsync(model.RentalId);
            if (rental == null) return NotFound();
            
            var business = await _businessService.GetBusinessByUserIdAsync(userId);
            if (business == null || rental.BusinessId != business.Id) return Forbid();

            await _rentalService.IssueEquipmentAsync(model.RentalId, userId, model.ConditionAtIssue, model.IssueNotes);
            TempData["SuccessMessage"] = "Equipment successfully issued.";
            return RedirectToAction(nameof(Index));
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [Authorize(Roles = "Business,VendorStaff")]
    public async Task<IActionResult> Return(int id)
    {
        var rental = await _rentalService.GetRentalByIdAsync(id);
        if (rental == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var business = await _businessService.GetBusinessByUserIdAsync(userId!);
        if (business == null || rental.BusinessId != business.Id) return Forbid();

        var model = new RentalReturnViewModel
        {
            RentalId = rental.Id,
            BookingNumber = rental.Booking.BookingNumber,
            CustomerName = rental.Customer.Email!, 
            EquipmentName = rental.Equipment.Name,
            EquipmentCode = rental.Equipment.EquipmentCode,
            ExpectedReturn = rental.ExpectedReturnDate,
            ConditionAtIssue = rental.ConditionAtIssue ?? RentKart.Core.Enums.EquipmentCondition.Good,
            ConditionAtReturn = rental.ConditionAtIssue ?? RentKart.Core.Enums.EquipmentCondition.Good
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Business,VendorStaff")]
    public async Task<IActionResult> Return(RentalReturnViewModel model)
    {
        if (model.DamageFound && string.IsNullOrWhiteSpace(model.DamageDescription))
        {
            ModelState.AddModelError("DamageDescription", "Damage description is required when damage is found.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            var rental = await _rentalService.GetRentalByIdAsync(model.RentalId);
            if (rental == null) return NotFound();
            
            var business = await _businessService.GetBusinessByUserIdAsync(userId);
            if (business == null || rental.BusinessId != business.Id) return Forbid();

            await _rentalService.ReturnEquipmentAsync(model.RentalId, userId, model.ConditionAtReturn, model.DamageFound, model.DamageDescription, model.ReturnNotes);
            TempData["SuccessMessage"] = "Equipment successfully returned.";
            return RedirectToAction(nameof(Index));
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}

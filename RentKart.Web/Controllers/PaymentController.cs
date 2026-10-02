using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;
using RentKart.Web.ViewModels.Payment;

namespace RentKart.Web.Controllers;

[Authorize(Roles = "Customer")]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IBookingService _bookingService;

    public PaymentController(IPaymentService paymentService, IBookingService bookingService)
    {
        _paymentService = paymentService;
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int bookingId)
    {
        var booking = await _bookingService.GetBookingByIdAsync(bookingId);
        if (booking == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (booking.CustomerId != userId) return Forbid();

        if (booking.Status != BookingStatus.Approved)
        {
            TempData["ErrorMessage"] = "This booking is not approved for payment yet.";
            return RedirectToAction("Details", "Booking", new { id = bookingId });
        }

        if (booking.PaymentStatus == PaymentStatus.Succeeded)
        {
            TempData["ErrorMessage"] = "Payment has already been completed for this booking.";
            return RedirectToAction("Details", "Booking", new { id = bookingId });
        }

        var viewModel = new PaymentCreateViewModel
        {
            BookingId = booking.Id,
            BookingNumber = booking.BookingNumber,
            EquipmentName = booking.Equipment.Name,
            BusinessName = booking.Business.BusinessName,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            RentalAmount = booking.RentalAmount,
            SecurityDeposit = booking.SecurityDepositAmount,
            TotalAmount = booking.TotalAmount
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(int bookingId, PaymentMethod paymentMethod)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var payment = await _paymentService.ProcessDemoPaymentAsync(bookingId, userId, paymentMethod);

            if (payment.PaymentStatus == PaymentStatus.Succeeded)
            {
                return RedirectToAction(nameof(Success), new { paymentReference = payment.PaymentReference });
            }
            else
            {
                return RedirectToAction(nameof(Failed), new { paymentReference = payment.PaymentReference });
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Booking", new { id = bookingId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Success(string paymentReference)
    {
        var payment = await _paymentService.GetPaymentByReferenceAsync(paymentReference);
        if (payment == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (payment.CustomerId != userId) return Forbid();

        return View(payment);
    }

    [HttpGet]
    public async Task<IActionResult> Failed(string paymentReference)
    {
        var payment = await _paymentService.GetPaymentByReferenceAsync(paymentReference);
        if (payment == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (payment.CustomerId != userId) return Forbid();

        return View(payment);
    }

    [HttpGet]
    public async Task<IActionResult> MyPayments()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var payments = await _paymentService.GetCustomerPaymentsAsync(userId);
        return View(payments);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (payment.CustomerId != userId) return Forbid();

        return View(payment);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;
using RentKart.Infrastructure.Data;

namespace RentKart.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IBookingService _bookingService;
    private readonly IRentalService _rentalService;
    private readonly INotificationService _notificationService;

    public PaymentService(ApplicationDbContext context, IPaymentGateway paymentGateway, IBookingService bookingService, IRentalService rentalService, INotificationService notificationService)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _bookingService = bookingService;
        _rentalService = rentalService;
        _notificationService = notificationService;
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Booking)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetPaymentByReferenceAsync(string paymentReference)
    {
        return await _context.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b.Equipment)
            .Include(p => p.Booking)
                .ThenInclude(b => b.Business)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.PaymentReference == paymentReference);
    }

    public async Task<IEnumerable<Payment>> GetCustomerPaymentsAsync(string customerId)
    {
        return await _context.Payments
            .Include(p => p.Booking)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetBusinessPaymentsAsync(int businessId)
    {
        return await _context.Payments
            .Include(p => p.Booking)
            .Where(p => p.Booking.BusinessId == businessId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Payment> ProcessDemoPaymentAsync(int bookingId, string customerId, PaymentMethod method)
    {
        // 1. Load Booking
        var booking = await _context.Bookings
            .Include(b => b.Equipment)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new InvalidOperationException("Booking not found.");

        // 2. Verify customer ownership
        if (booking.CustomerId != customerId)
            throw new InvalidOperationException("Unauthorized access to booking.");

        // 3. Verify booking status
        if (booking.Status != BookingStatus.Approved)
            throw new InvalidOperationException($"Cannot pay for booking with status: {booking.Status}");

        // 4. Verify payment eligibility
        if (booking.PaymentStatus == PaymentStatus.Succeeded)
            throw new InvalidOperationException("Payment has already been completed.");

        // 5. Calculate trusted amount
        decimal amount = booking.TotalAmount;

        // 6. Create Payment
        var payment = new Payment
        {
            PaymentReference = $"RKPAY-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            BookingId = booking.Id,
            CustomerId = customerId,
            Amount = amount,
            Currency = "INR",
            PaymentMethod = method,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // 7. Call Gateway
        var paymentRequest = new PaymentRequest
        {
            Amount = amount,
            Currency = "INR",
            Method = method,
            PaymentReference = payment.PaymentReference
        };

        var result = await _paymentGateway.ProcessPaymentAsync(paymentRequest);

        // 9. Update Payment Status
        payment.PaymentStatus = result.Status;
        payment.TransactionReference = result.TransactionReference;
        
        if (result.IsSuccess)
        {
            payment.PaidAt = DateTime.UtcNow;

            // 10. Update Booking payment status
            booking.PaymentStatus = PaymentStatus.Succeeded;
            booking.PaidAmount = amount;
            booking.PaidAt = DateTime.UtcNow;
            
            // Note: If you want Booking to also become "Confirmed", you can do it here. 
            // The spec mentions preferred state: BookingStatus: Approved, PaymentStatus: Succeeded. So we'll just set PaymentStatus.
            
            // Phase 9: Prepare rental for pickup
            await _rentalService.PrepareForPickupAsync(booking.Id);
        }

        payment.UpdatedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        if (result.IsSuccess)
        {
            var invoice = new Invoice
            {
                BookingId = booking.Id,
                InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0,6).ToUpper()}",
                IssuedAt = DateTime.UtcNow,
                Subtotal = booking.RentalAmount,
                SecurityDeposit = booking.SecurityDepositAmount,
                TaxAmount = 0, // Simplified tax
                TotalAmount = booking.TotalAmount,
                Status = "Paid"
            };
            _context.Invoices.Add(invoice);
        }
        
        // 11. Save changes
        await _context.SaveChangesAsync();

        if (result.IsSuccess)
        {
            await _notificationService.CreateNotificationAsync(
                customerId,
                NotificationType.PaymentSuccessful,
                "Payment Successful",
                $"Payment of ₹{amount:N2} for booking {booking.BookingNumber} was successful.",
                "Payment",
                payment.Id.ToString(),
                $"/Booking/Details/{bookingId}");
        }
        else
        {
            await _notificationService.CreateNotificationAsync(
                customerId,
                NotificationType.PaymentFailed,
                "Payment Failed",
                $"Your payment for booking {booking.BookingNumber} was unsuccessful. You can try again.",
                "Payment",
                payment.Id.ToString(),
                $"/Booking/Details/{bookingId}");
        }

        return payment;
    }
}

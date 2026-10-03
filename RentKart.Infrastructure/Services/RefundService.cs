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

public class RefundService : IRefundService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public RefundService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Refund> ProcessRefundAsync(int paymentId, string customerId, decimal amount, string reason, string staffId)
    {
        var payment = await _context.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) throw new Exception("Payment not found");
        
        if (payment.PaymentStatus != PaymentStatus.Succeeded && payment.PaymentStatus != PaymentStatus.PartiallyRefunded)
            throw new Exception("Only successful payments can be refunded.");
            
        var totalRefunded = await _context.Refunds
            .Where(r => r.PaymentId == paymentId)
            .SumAsync(r => r.Amount);
            
        if (amount + totalRefunded > payment.Amount)
            throw new Exception("Refund amount exceeds the original payment amount.");

        var refund = new Refund
        {
            PaymentId = paymentId,
            BookingId = payment.BookingId,
            CustomerId = payment.CustomerId,
            Amount = amount,
            Reason = reason,
            Status = "Processed",
            RefundReference = $"RKREF-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0,8).ToUpper()}",
            CreatedAt = DateTime.UtcNow
        };

        _context.Refunds.Add(refund);
        
        if (amount + totalRefunded == payment.Amount)
        {
            payment.PaymentStatus = PaymentStatus.Refunded;
        }
        else
        {
            payment.PaymentStatus = PaymentStatus.PartiallyRefunded; // Assuming it exists, otherwise we just leave it or use something else. Wait, let me check if PartiallyRefunded exists.
        }
        
        payment.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        await _notificationService.CreateNotificationAsync(
            payment.CustomerId,
            NotificationType.System,
            "Refund Processed",
            $"A refund of ?{amount:N2} has been processed for your booking.",
            "Payment",
            payment.Id.ToString()
        );

        return refund;
    }

    public async Task<IEnumerable<Refund>> GetCustomerRefundsAsync(string customerId)
    {
        return await _context.Refunds
            .Include(r => r.Booking)
                .ThenInclude(b => b.Equipment)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Refund>> GetBusinessRefundsAsync(int businessId)
    {
        return await _context.Refunds
            .Include(r => r.Booking)
                .ThenInclude(b => b.Customer)
            .Where(r => r.Booking.BusinessId == businessId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}

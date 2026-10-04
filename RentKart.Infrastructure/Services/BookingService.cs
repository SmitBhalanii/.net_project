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

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public BookingService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Booking> CreateBookingAsync(Booking booking)
    {
        // 1. Verify equipment exists and is active
        var equipment = await _context.Equipment
            .Include(e => e.Business)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == booking.EquipmentId);

        if (equipment == null || !equipment.IsActive)
        {
            throw new InvalidOperationException("Equipment is not available.");
        }

        // 2. Verify business is approved
        if (equipment.Business.ApprovalStatus != BusinessApprovalStatus.Approved)
        {
            throw new InvalidOperationException("Business is not approved.");
        }

        // 3. Verify category is active
        if (!equipment.Category.IsActive)
        {
            throw new InvalidOperationException("Category is not active.");
        }

        // 4. Verify availability
        var isAvailable = await IsEquipmentAvailableAsync(booking.EquipmentId, booking.StartDate, booking.EndDate);
        if (!isAvailable)
        {
            throw new InvalidOperationException("Equipment is not available for the selected dates.");
        }

        // 5. Calculate amounts server-side
        int rentalDays = (booking.EndDate - booking.StartDate).Days + 1;
        if (rentalDays <= 0)
        {
            throw new InvalidOperationException("Invalid rental dates.");
        }

        booking.Quantity = 1; // Default MVP logic
        booking.DailyRate = equipment.RentalPrice;
        booking.RentalAmount = booking.DailyRate * rentalDays * booking.Quantity;
        booking.SecurityDepositAmount = equipment.SecurityDeposit * booking.Quantity;
        booking.TotalAmount = booking.RentalAmount + booking.SecurityDepositAmount;

        booking.BusinessId = equipment.BusinessId;
        booking.Status = BookingStatus.Pending;
        booking.BookingNumber = GenerateBookingNumber();
        booking.CreatedAt = DateTime.UtcNow;

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.CustomerId,
                NotificationType.BookingCreated,
                "Booking Request Sent",
                $"Your booking {booking.BookingNumber} has been sent to {equipment.Business.BusinessName}.",
                "Booking",
                booking.Id.ToString(),
                $"/Booking/Details/{booking.Id}");

        // Notify Business Admin
        await _notificationService.CreateNotificationAsync(
            equipment.Business.UserId,
            NotificationType.BookingCreated,
            "New Booking Request",
            $"New booking request {booking.BookingNumber} received for {equipment.Name}.",
            "Booking",
            booking.Id.ToString(),
            "/BusinessBooking");

        return booking;
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Equipment)
            .Include(b => b.Business)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking?> GetBookingByNumberAsync(string bookingNumber)
    {
        return await _context.Bookings
            .Include(b => b.Equipment)
            .Include(b => b.Business)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);
    }

    public async Task<IEnumerable<Booking>> GetCustomerBookingsAsync(string customerId)
    {
        return await _context.Bookings
            .Include(b => b.Equipment)
            .Include(b => b.Business)
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBusinessBookingsAsync(int businessId)
    {
        return await _context.Bookings
            .Include(b => b.Equipment)
            .Include(b => b.Customer)
            .Where(b => b.BusinessId == businessId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsEquipmentAvailableAsync(int equipmentId, DateTime startDate, DateTime endDate)
    {
        // Overlap condition: Existing.StartDate <= Requested.EndDate AND Existing.EndDate >= Requested.StartDate
        var blockingStatuses = new[] { BookingStatus.Pending, BookingStatus.Approved, BookingStatus.Active };

        var overlappingBookingExists = await _context.Bookings
            .Where(b => b.EquipmentId == equipmentId)
            .Where(b => blockingStatuses.Contains(b.Status))
            .AnyAsync(b => b.StartDate <= endDate && b.EndDate >= startDate);

        return !overlappingBookingExists;
    }

    public async Task<bool> ApproveBookingAsync(int id, int businessId, string? businessNote)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.BusinessId == businessId);
        if (booking == null) return false;

        if (booking.Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException("Only pending bookings can be approved.");
        }

        // Re-check availability before approval
        var isAvailable = await IsEquipmentAvailableAsync(booking.EquipmentId, booking.StartDate, booking.EndDate);
        
        // Wait, the current booking itself is Pending, so it will be considered overlapping with itself if we don't exclude it!
        // We need to adjust IsEquipmentAvailableAsync or do a custom check here.
        var blockingStatuses = new[] { BookingStatus.Pending, BookingStatus.Approved, BookingStatus.Active };
        var otherOverlappingBookingExists = await _context.Bookings
            .Where(b => b.EquipmentId == booking.EquipmentId && b.Id != booking.Id)
            .Where(b => blockingStatuses.Contains(b.Status))
            .AnyAsync(b => b.StartDate <= booking.EndDate && b.EndDate >= booking.StartDate);

        if (otherOverlappingBookingExists)
        {
             throw new InvalidOperationException("Equipment is no longer available for the selected dates.");
        }

        booking.Status = BookingStatus.Approved;
        booking.BusinessNote = businessNote;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var equipment = await _context.Equipment.Include(e => e.Business).FirstOrDefaultAsync(e => e.Id == booking.EquipmentId);

        if (equipment != null)
        {
            await _notificationService.CreateNotificationAsync(
                booking.CustomerId,
                NotificationType.BookingApproved,
                "Booking Approved",
                $"Your booking {booking.BookingNumber} has been approved by {equipment.Business.BusinessName}.",
                "Booking",
                booking.Id.ToString(),
                $"/Booking/Details/{booking.Id}");

            await _notificationService.CreateNotificationAsync(
                booking.CustomerId,
                NotificationType.PaymentRequired,
                "Payment Required",
                $"Your booking {booking.BookingNumber} is approved. Please complete payment to confirm your rental.",
                "Booking",
                booking.Id.ToString(),
                $"/Booking/Details/{booking.Id}");
        }

        return true;
    }

    public async Task<bool> RejectBookingAsync(int id, int businessId, string? businessNote)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.BusinessId == businessId);
        if (booking == null) return false;

        if (booking.Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException("Only pending bookings can be rejected.");
        }

        booking.Status = BookingStatus.Rejected;
        booking.BusinessNote = businessNote;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var equipment = await _context.Equipment.Include(e => e.Business).FirstOrDefaultAsync(e => e.Id == booking.EquipmentId);
        
        if (equipment != null)
        {
            string message = $"Your booking {booking.BookingNumber} was rejected by {equipment.Business.BusinessName}.";
            if (!string.IsNullOrWhiteSpace(businessNote))
            {
                message += $" Reason: {businessNote}";
            }

            await _notificationService.CreateNotificationAsync(
                booking.CustomerId,
                NotificationType.BookingRejected,
                "Booking Rejected",
                message,
                "Booking",
                booking.Id.ToString(),
                $"/Booking/Details/{booking.Id}");
        }

        return true;
    }

    public async Task<bool> CancelBookingAsync(int id, string customerId, string? customerNote)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == customerId);
        if (booking == null) return false;

        if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Approved)
        {
            throw new InvalidOperationException("Only pending or approved bookings can be cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        
        if (!string.IsNullOrWhiteSpace(customerNote))
        {
             booking.CustomerNote = string.IsNullOrWhiteSpace(booking.CustomerNote) 
                ? customerNote 
                : $"{booking.CustomerNote} | Cancel Note: {customerNote}";
        }
        
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(
            booking.CustomerId,
            NotificationType.BookingCancelled,
            "Booking Cancelled",
            $"Your booking {booking.BookingNumber} has been cancelled.",
            "Booking",
            booking.Id.ToString(),
            $"/Booking/Details/{booking.Id}");

        var equipment = await _context.Equipment.Include(e => e.Business).FirstOrDefaultAsync(e => e.Id == booking.EquipmentId);
        if (equipment != null)
        {
            await _notificationService.CreateNotificationAsync(
                equipment.Business.UserId,
                NotificationType.BookingCancelled,
                "Booking Cancelled",
                $"Booking {booking.BookingNumber} has been cancelled by the customer.",
                "Booking",
                booking.Id.ToString(),
                "/BusinessBooking");
        }

        return true;
    }

    public string GenerateBookingNumber()
    {
        return $"RK-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}

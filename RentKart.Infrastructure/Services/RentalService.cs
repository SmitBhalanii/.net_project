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

public class RentalService : IRentalService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public RentalService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Rental?> GetRentalByIdAsync(int rentalId)
    {
        return await _context.Rentals
            .Include(r => r.Booking)
            .Include(r => r.Equipment)
            .Include(r => r.Customer)
            .Include(r => r.Business)
            .FirstOrDefaultAsync(r => r.Id == rentalId);
    }

    public async Task<Rental?> GetRentalByBookingIdAsync(int bookingId)
    {
        return await _context.Rentals
            .Include(r => r.Booking)
            .Include(r => r.Equipment)
            .Include(r => r.Customer)
            .Include(r => r.Business)
            .FirstOrDefaultAsync(r => r.BookingId == bookingId);
    }

    public async Task<IEnumerable<Rental>> GetBusinessRentalsAsync(int businessId)
    {
        return await _context.Rentals
            .Include(r => r.Booking)
            .Include(r => r.Equipment)
            .Include(r => r.Customer)
            .Where(r => r.BusinessId == businessId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rental>> GetCustomerRentalsAsync(string customerId)
    {
        return await _context.Rentals
            .Include(r => r.Booking)
            .Include(r => r.Equipment)
            .Include(r => r.Business)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Rental> PrepareForPickupAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Equipment)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new Exception("Booking not found");

        if (booking.Status != BookingStatus.Approved)
            throw new Exception("Booking is not approved");

        if (booking.PaymentStatus != PaymentStatus.Succeeded)
            throw new Exception("Booking is not paid");

        var existingRental = await _context.Rentals.FirstOrDefaultAsync(r => r.BookingId == bookingId);
        if (existingRental != null)
            return existingRental;

        var rental = new Rental
        {
            BookingId = booking.Id,
            EquipmentId = booking.EquipmentId,
            CustomerId = booking.CustomerId,
            BusinessId = booking.BusinessId,
            Status = RentalStatus.ReadyForPickup,
            ExpectedReturnDate = booking.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();

        var equipment = await _context.Equipment.Include(e => e.Business).FirstOrDefaultAsync(e => e.Id == booking.EquipmentId);
        if (equipment != null)
        {
            await _notificationService.CreateNotificationAsync(
                booking.CustomerId,
                NotificationType.PickupReady,
                "Ready for Pickup",
                $"Your {equipment.Name} is ready for pickup at {equipment.Business.BusinessName}.",
                "Booking",
                booking.Id.ToString());
        }

        return rental;
    }

    public async Task<Rental> IssueEquipmentAsync(int rentalId, string staffId, EquipmentCondition condition, string? notes)
    {
        var rental = await GetRentalByIdAsync(rentalId);
        if (rental == null) throw new Exception("Rental not found");

        if (rental.Status != RentalStatus.ReadyForPickup)
            throw new Exception("Rental is not ready for pickup");

        if (rental.Equipment.Status == EquipmentStatus.UnderMaintenance || rental.Equipment.Status == EquipmentStatus.Inactive || rental.Equipment.Status == EquipmentStatus.Rented)
            throw new Exception("Equipment is not available for issue");

        // Use Date instead of exact time since it's college MVP and picking up earlier on the same day is fine
        if (DateTime.UtcNow.Date < rental.Booking.StartDate.Date)
            throw new Exception("Equipment cannot be issued before the rental start date.");

        rental.Status = RentalStatus.Active;
        rental.IssuedAt = DateTime.UtcNow;
        rental.IssuedByStaffId = staffId;
        rental.ConditionAtIssue = condition;
        rental.IssueNotes = notes;
        rental.UpdatedAt = DateTime.UtcNow;

        rental.Booking.Status = BookingStatus.Active;
        rental.Booking.UpdatedAt = DateTime.UtcNow;

        rental.Equipment.Status = EquipmentStatus.Rented;
        rental.Equipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == rental.EquipmentId);
        if (equipment != null)
        {
            await _notificationService.CreateNotificationAsync(
                rental.CustomerId,
                NotificationType.EquipmentIssued,
                "Equipment Issued",
                $"{equipment.Name} has been successfully issued to you. Rental is now active.",
                "Rental",
                rental.Id.ToString());
                
            await _notificationService.CreateNotificationAsync(
                rental.CustomerId,
                NotificationType.RentalStarted,
                "Rental Started",
                $"Your rental for {equipment.Name} has started. Expected return: {rental.ExpectedReturnDate:dd MMM yyyy}",
                "Rental",
                rental.Id.ToString());
        }

        return rental;
    }

    public async Task<Rental> ReturnEquipmentAsync(int rentalId, string staffId, EquipmentCondition condition, bool damageFound, string? damageDescription, string? notes)
    {
        var rental = await GetRentalByIdAsync(rentalId);
        if (rental == null) throw new Exception("Rental not found");

        if (rental.Status != RentalStatus.Active)
            throw new Exception("Rental is not active");

        rental.Status = RentalStatus.Returned;
        rental.ActualReturnDate = DateTime.UtcNow;
        rental.ReturnedToStaffId = staffId;
        rental.ConditionAtReturn = condition;
        rental.DamageFound = damageFound;
        rental.DamageDescription = damageDescription;
        rental.ReturnNotes = notes;
        rental.UpdatedAt = DateTime.UtcNow;

        // In this MVP, Completed marks the end of the rental lifecycle
        rental.Status = RentalStatus.Completed; 
        
        rental.Booking.Status = BookingStatus.Completed;
        rental.Booking.UpdatedAt = DateTime.UtcNow;

        if (damageFound)
        {
            rental.Equipment.Status = EquipmentStatus.UnderMaintenance;
        }
        else
        {
            rental.Equipment.Status = EquipmentStatus.Active; // Active mapping to Available
        }
        rental.Equipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == rental.EquipmentId);
        if (equipment != null)
        {
            await _notificationService.CreateNotificationAsync(
                rental.CustomerId,
                NotificationType.EquipmentReturned,
                "Equipment Returned",
                $"{equipment.Name} has been successfully returned.",
                "Rental",
                rental.Id.ToString());
                
            await _notificationService.CreateNotificationAsync(
                rental.CustomerId,
                NotificationType.RentalCompleted,
                "Rental Completed",
                $"Your rental of {equipment.Name} has been completed. You can now leave a review.",
                "Rental",
                rental.Id.ToString());
        }

        return rental;
    }

    public async Task<Equipment?> GetEquipmentByCodeAsync(string equipmentCode)
    {
        return await _context.Equipment
            .Include(e => e.Business)
            .FirstOrDefaultAsync(e => e.EquipmentCode == equipmentCode);
    }

    public async Task ProcessDueNotificationsAsync()
    {
        var activeRentals = await _context.Rentals
            .Include(r => r.Equipment)
            .Include(r => r.Business)
            .Where(r => r.Status == RentalStatus.Active)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;

        foreach (var rental in activeRentals)
        {
            var expectedDate = rental.ExpectedReturnDate.Date;
            
            if (expectedDate == today.AddDays(1))
            {
                // Check if notification already exists
                var exists = await _context.Notifications.AnyAsync(n => 
                    n.UserId == rental.CustomerId && 
                    n.RelatedEntityId == rental.Id.ToString() && 
                    n.NotificationType == NotificationType.RentalDueSoon);
                    
                if (!exists)
                {
                    await _notificationService.CreateNotificationAsync(
                        rental.CustomerId,
                        NotificationType.RentalDueSoon,
                        "Rental Due Tomorrow",
                        $"Your {rental.Equipment.Name} rental is due tomorrow. Please return it to {rental.Business.BusinessName} on time.",
                        "Rental",
                        rental.Id.ToString());
                }
            }
            else if (today > expectedDate)
            {
                int daysOverdue = (today - expectedDate).Days;
                
                var latestOverdueNotification = await _context.Notifications
                    .Where(n => 
                        n.UserId == rental.CustomerId && 
                        n.RelatedEntityId == rental.Id.ToString() && 
                        n.NotificationType == NotificationType.RentalOverdue)
                    .OrderByDescending(n => n.CreatedAt)
                    .FirstOrDefaultAsync();
                    
                // Generate if none, or if it has been more than 24 hours since the last overdue notification
                if (latestOverdueNotification == null || latestOverdueNotification.CreatedAt.Date < today)
                {
                    await _notificationService.CreateNotificationAsync(
                        rental.CustomerId,
                        NotificationType.RentalOverdue,
                        "Rental Overdue",
                        $"Your {rental.Equipment.Name} rental is overdue by {daysOverdue} days. Please return the equipment to {rental.Business.BusinessName}.",
                        "Rental",
                        rental.Id.ToString());
                        
                    await _notificationService.CreateNotificationAsync(
                        rental.Business.UserId,
                        NotificationType.RentalOverdue,
                        "Rental Overdue",
                        $"{rental.Equipment.Name} rental is overdue by {daysOverdue} days.",
                        "Rental",
                        rental.Id.ToString());
                }
            }
        }
    }
}

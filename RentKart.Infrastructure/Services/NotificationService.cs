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

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IRealTimeNotifier? _realTimeNotifier;

    public NotificationService(ApplicationDbContext context, IRealTimeNotifier? realTimeNotifier = null)
    {
        _context = context;
        _realTimeNotifier = realTimeNotifier;
    }

    public async Task CreateNotificationAsync(string userId, NotificationType type, string title, string message, string? relatedEntityType = null, string? relatedEntityId = null, string? actionUrl = null)
    {
        // Duplicate Prevention
        if (relatedEntityId != null)
        {
            var exists = await _context.Notifications.AnyAsync(n => 
                n.UserId == userId && 
                n.NotificationType == type && 
                n.RelatedEntityId == relatedEntityId && 
                n.RelatedEntityType == relatedEntityType &&
                n.CreatedAt > DateTime.UtcNow.AddMinutes(-5));

            if (exists) return;
        }

        // Check Preferences
        var prefs = await GetPreferencesAsync(userId);
        if (!ShouldSendNotification(type, prefs)) return;

        var notification = new Notification
        {
            UserId = userId,
            NotificationType = type,
            Title = title,
            Message = message,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        if (_realTimeNotifier != null)
        {
            var count = await GetUnreadCountAsync(userId);
            await _realTimeNotifier.SendNotificationAsync(userId, title, message, actionUrl);
            await _realTimeNotifier.UpdateUnreadCountAsync(userId, count);
        }
    }

    private bool ShouldSendNotification(NotificationType type, NotificationPreference prefs)
    {
        return type switch
        {
            NotificationType.BookingCreated or NotificationType.BookingApproved or NotificationType.BookingRejected or NotificationType.BookingCancelled => prefs.BookingUpdates,
            NotificationType.PaymentRequired or NotificationType.PaymentSuccessful or NotificationType.PaymentFailed => prefs.PaymentUpdates,
            NotificationType.RentalStarted or NotificationType.RentalDueSoon or NotificationType.RentalOverdue or NotificationType.RentalCompleted or NotificationType.PickupReady or NotificationType.EquipmentIssued or NotificationType.EquipmentReturned => prefs.RentalReminders,
            NotificationType.ReviewSubmitted or NotificationType.ReviewApproved or NotificationType.ReviewRejected => prefs.ReviewReminders,
            _ => true,
        };
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, bool? unreadOnly = null)
    {
        var query = _context.Notifications.AsNoTracking().Where(n => n.UserId == userId);

        if (unreadOnly.HasValue && unreadOnly.Value)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications.AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(string userId, int notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            if (_realTimeNotifier != null)
            {
                var count = await GetUnreadCountAsync(userId);
                await _realTimeNotifier.UpdateUnreadCountAsync(userId, count);
            }
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        // Optimize Mark All as Read using ExecuteUpdateAsync
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTime.UtcNow));
                
        if (_realTimeNotifier != null)
        {
            await _realTimeNotifier.UpdateUnreadCountAsync(userId, 0);
        }
    }

    public async Task DeleteAsync(string userId, int notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            
            if (!notification.IsRead && _realTimeNotifier != null)
            {
                var count = await GetUnreadCountAsync(userId);
                await _realTimeNotifier.UpdateUnreadCountAsync(userId, count);
            }
        }
    }

    public async Task<NotificationPreference> GetPreferencesAsync(string userId)
    {
        var pref = await _context.NotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (pref == null)
        {
            pref = new NotificationPreference { UserId = userId };
            _context.NotificationPreferences.Add(pref);
            await _context.SaveChangesAsync();
        }

        return pref;
    }

    public async Task UpdatePreferencesAsync(NotificationPreference preference)
    {
        var existing = await _context.NotificationPreferences.FirstOrDefaultAsync(p => p.UserId == preference.UserId);
        if (existing != null)
        {
            existing.BookingUpdates = preference.BookingUpdates;
            existing.PaymentUpdates = preference.PaymentUpdates;
            existing.RentalReminders = preference.RentalReminders;
            existing.ReviewReminders = preference.ReviewReminders;
            existing.MarketingPromotional = preference.MarketingPromotional;
        }
        else
        {
            _context.NotificationPreferences.Add(preference);
        }
        await _context.SaveChangesAsync();
    }
}



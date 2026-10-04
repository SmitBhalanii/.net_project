using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;
using RentKart.Core.Enums;

namespace RentKart.Core.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, NotificationType type, string title, string message, string? relatedEntityType = null, string? relatedEntityId = null, string? actionUrl = null);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, bool? unreadOnly = null);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(string userId, int notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task DeleteAsync(string userId, int notificationId);
    Task<NotificationPreference> GetPreferencesAsync(string userId);
    Task UpdatePreferencesAsync(NotificationPreference preference);
}

using System;
using System.Collections.Generic;
using RentKart.Core.Enums;

namespace RentKart.Web.ViewModels.Notification;

public class NotificationViewModel
{
    public int Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationListViewModel
{
    public IEnumerable<NotificationViewModel> Notifications { get; set; } = new List<NotificationViewModel>();
    public int UnreadCount { get; set; }
}
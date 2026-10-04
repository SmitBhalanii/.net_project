using System;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class Notification
{
    public int Id { get; set; }
    
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    
    public NotificationType NotificationType { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    
    public bool IsRead { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

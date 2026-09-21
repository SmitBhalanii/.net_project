using System;

namespace RentKart.Core.Entities;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

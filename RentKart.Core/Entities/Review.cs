using System;

namespace RentKart.Core.Entities;

public class Review
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = null!;
    
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

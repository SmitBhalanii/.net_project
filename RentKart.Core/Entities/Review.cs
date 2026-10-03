using System;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class Review
{
    public int Id { get; set; }
    
    public string CustomerId { get; set; } = null!;
    public ApplicationUser Customer { get; set; } = null!;
    
    public int RentalId { get; set; }
    public Rental Rental { get; set; } = null!;
    
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    
    public int EquipmentRating { get; set; }
    public int BusinessRating { get; set; }
    
    public string Title { get; set; } = null!;
    public string Comment { get; set; } = null!;
    public ReviewStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

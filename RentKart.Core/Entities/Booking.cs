using System;
using System.Collections.Generic;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class Booking
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = null!;
    
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PickupDate { get; set; }
    public TimeSpan PickupTime { get; set; }
    public string PickupLocation { get; set; } = null!;

    public decimal Subtotal { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal TotalAmount { get; set; }

    public BookingStatus Status { get; set; }
    
    public string? CustomerNotes { get; set; }
    public string? BusinessNotes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    
    public RentalAgreement? RentalAgreement { get; set; }
    public Invoice? Invoice { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();
}

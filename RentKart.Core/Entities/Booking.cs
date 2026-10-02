using System;
using System.Collections.Generic;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class Booking
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    
    public string CustomerId { get; set; } = null!;
    public ApplicationUser Customer { get; set; } = null!;
    
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Quantity { get; set; } = 1;
    
    public decimal DailyRate { get; set; }
    public decimal RentalAmount { get; set; }
    public decimal SecurityDepositAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public BookingStatus Status { get; set; }
    
    public string? CustomerNote { get; set; }
    public string? BusinessNote { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public RentalAgreement? RentalAgreement { get; set; }
    public Invoice? Invoice { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();
}

using System;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class Rental
{
    public int Id { get; set; }
    
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    
    public string CustomerId { get; set; } = null!;
    public ApplicationUser Customer { get; set; } = null!;
    
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    
    public string? IssuedByStaffId { get; set; }
    public ApplicationUser? IssuedByStaff { get; set; }
    
    public string? ReturnedToStaffId { get; set; }
    public ApplicationUser? ReturnedToStaff { get; set; }
    
    public DateTime? IssuedAt { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    
    public string? IssueNotes { get; set; }
    public string? ReturnNotes { get; set; }
    
    public EquipmentCondition? ConditionAtIssue { get; set; }
    public EquipmentCondition? ConditionAtReturn { get; set; }
    
    public RentalStatus Status { get; set; }
    
    public bool DamageFound { get; set; }
    public string? DamageDescription { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

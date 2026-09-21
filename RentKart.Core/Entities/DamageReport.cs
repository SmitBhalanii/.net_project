using System;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class DamageReport
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    
    public string ReportedByUserId { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal? EstimatedCost { get; set; }
    
    public DamageStatus Status { get; set; }
    public string? EvidenceImagePath { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

using System;
using System.Collections.Generic;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class Equipment
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string? Description { get; set; }
    public string? Specifications { get; set; }
    
    public decimal RentalPricePerDay { get; set; }
    public decimal SecurityDeposit { get; set; }
    public int Quantity { get; set; }
    
    public EquipmentCondition Condition { get; set; }
    public EquipmentStatus Status { get; set; }
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<EquipmentImage> EquipmentImages { get; set; } = new List<EquipmentImage>();
    public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
    public ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();
}

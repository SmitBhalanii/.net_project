using System;
using System.Collections.Generic;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class Business
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public string BusinessName { get; set; } = null!;
    public string OwnerName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoPath { get; set; }
    public string? CoverImagePath { get; set; }
    public string? BusinessHours { get; set; }
    public BusinessApprovalStatus ApprovalStatus { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

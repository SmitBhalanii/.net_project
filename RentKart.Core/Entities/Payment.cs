using System;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class Payment
{
    public int Id { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public string CustomerId { get; set; } = null!;
    public ApplicationUser Customer { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    
    public string? TransactionReference { get; set; }
    
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

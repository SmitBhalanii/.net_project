using System;
using RentKart.Core.Enums;

namespace RentKart.Core.Entities;

public class Refund
{
    public int Id { get; set; }
    
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public string CustomerId { get; set; } = null!;
    public ApplicationUser Customer { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Processed";
    
    public string? RefundReference { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

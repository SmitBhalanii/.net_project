using System;

namespace RentKart.Core.Entities;

public class Invoice
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public string InvoiceNumber { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
    
    public decimal Subtotal { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public string Status { get; set; } = null!;
    public string? DocumentPath { get; set; }
}

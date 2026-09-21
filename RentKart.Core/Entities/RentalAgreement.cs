using System;

namespace RentKart.Core.Entities;

public class RentalAgreement
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public string AgreementNumber { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
    public string TermsAndConditions { get; set; } = null!;
    
    public DateTime? CustomerAcknowledgedAt { get; set; }
    public DateTime? BusinessAcknowledgedAt { get; set; }
    public string? DocumentPath { get; set; }
}

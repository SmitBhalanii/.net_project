using System;
using RentKart.Core.Enums;

namespace RentKart.Web.ViewModels.Booking;

public class BookingDetailsViewModel
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RentalDays { get; set; }
    public decimal DailyRate { get; set; }
    public decimal RentalAmount { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? CustomerNote { get; set; }
    public string? BusinessNote { get; set; }
    public DateTime CreatedAt { get; set; }
}

using System;
using RentKart.Core.Enums;

namespace RentKart.Web.ViewModels.Booking;

public class BusinessBookingViewModel
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal RentalAmount { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public string? CustomerNote { get; set; }
}

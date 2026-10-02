using System;
using RentKart.Core.Enums;

namespace RentKart.Web.ViewModels.Booking;

public class BookingListViewModel
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
}

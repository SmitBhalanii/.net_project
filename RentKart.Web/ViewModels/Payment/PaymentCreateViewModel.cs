using System;
using RentKart.Core.Enums;

namespace RentKart.Web.ViewModels.Payment;

public class PaymentCreateViewModel
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public decimal RentalAmount { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal TotalAmount { get; set; }
    
    public PaymentMethod SelectedMethod { get; set; }
}

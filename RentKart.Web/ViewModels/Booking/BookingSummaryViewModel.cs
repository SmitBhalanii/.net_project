using System;
using System.ComponentModel.DataAnnotations;

namespace RentKart.Web.ViewModels.Booking;

public class BookingSummaryViewModel
{
    public int EquipmentId { get; set; }
    
    public string EquipmentName { get; set; } = string.Empty;
    public string EquipmentImageUrl { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string PickupLocation { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RentalDays { get; set; }
    
    public decimal DailyRate { get; set; }
    public decimal RentalAmount { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal TotalAmount { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerNote { get; set; } = string.Empty;

    [Required]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the rental terms.")]
    public bool TermsAccepted { get; set; }
}

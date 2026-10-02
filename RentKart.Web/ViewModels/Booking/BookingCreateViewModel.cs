using System;
using System.ComponentModel.DataAnnotations;

namespace RentKart.Web.ViewModels.Booking;

public class BookingCreateViewModel
{
    [Required]
    public int EquipmentId { get; set; }
    
    public string EquipmentName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }
    public decimal SecurityDeposit { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

    [MaxLength(500)]
    [Display(Name = "Customer Note (Optional)")]
    public string? CustomerNote { get; set; }
}

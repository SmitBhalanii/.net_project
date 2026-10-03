using System;
using System.ComponentModel.DataAnnotations;
using RentKart.Core.Enums;

namespace RentKart.Web.ViewModels.Rental;

public class RentalIssueViewModel
{
    public int RentalId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string EquipmentCode { get; set; } = string.Empty;
    public DateTime RentalStart { get; set; }
    public DateTime RentalEnd { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    
    [Required]
    [Display(Name = "Condition at Issue")]
    public EquipmentCondition ConditionAtIssue { get; set; }
    
    [Display(Name = "Issue Notes")]
    [MaxLength(500)]
    public string? IssueNotes { get; set; }
}

public class RentalReturnViewModel
{
    public int RentalId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string EquipmentCode { get; set; } = string.Empty;
    public DateTime ExpectedReturn { get; set; }
    
    [Display(Name = "Condition at Issue")]
    public EquipmentCondition ConditionAtIssue { get; set; }
    
    [Required]
    [Display(Name = "Condition at Return")]
    public EquipmentCondition ConditionAtReturn { get; set; }
    
    [Required]
    [Display(Name = "Damage Found")]
    public bool DamageFound { get; set; }
    
    [Display(Name = "Damage Description")]
    [MaxLength(1000)]
    public string? DamageDescription { get; set; }
    
    [Display(Name = "Return Notes")]
    [MaxLength(500)]
    public string? ReturnNotes { get; set; }
}

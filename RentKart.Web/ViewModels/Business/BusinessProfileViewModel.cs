using System.Collections.Generic;
using RentKart.Core.Enums;
using RentKart.Web.ViewModels.Equipment;

namespace RentKart.Web.ViewModels.Business;

public class BusinessProfileViewModel
{
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoPath { get; set; }
    public string? CoverImagePath { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public BusinessApprovalStatus ApprovalStatus { get; set; }
    public bool IsApproved => ApprovalStatus == BusinessApprovalStatus.Approved;
    public int EquipmentCount { get; set; }
    
    // Using EquipmentCardViewModel to display equipment
    public IEnumerable<EquipmentCardViewModel> Equipment { get; set; } = new List<EquipmentCardViewModel>();
}

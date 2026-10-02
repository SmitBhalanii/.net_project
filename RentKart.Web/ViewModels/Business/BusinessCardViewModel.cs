using RentKart.Core.Enums;

namespace RentKart.Web.ViewModels.Business;

public class BusinessCardViewModel
{
    public int Id { get; set; }
    public string BusinessName { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoPath { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public BusinessApprovalStatus ApprovalStatus { get; set; }
    public bool IsApproved => ApprovalStatus == BusinessApprovalStatus.Approved;
    public int EquipmentCount { get; set; }
}

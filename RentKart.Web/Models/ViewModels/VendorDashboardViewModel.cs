namespace RentKart.Web.Models.ViewModels;

public class VendorDashboardViewModel
{
    public int TotalEquipment { get; set; }
    public int ActiveEquipment { get; set; }
    public int PendingBookings { get; set; }
    
    public int ReadyForPickup { get; set; }
    public int ActiveRentals { get; set; }
    public int TodayReturns { get; set; }
    public int OverdueRentals { get; set; }
}

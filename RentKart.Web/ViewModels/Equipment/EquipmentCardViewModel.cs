namespace RentKart.Web.ViewModels.Equipment;

public class EquipmentCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceUnit { get; set; } = "day";
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
}

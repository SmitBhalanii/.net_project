namespace RentKart.Web.ViewModels.Home;

using RentKart.Web.ViewModels.Equipment;
using RentKart.Web.ViewModels.Category;

public class HomePageViewModel
{
    public IEnumerable<CategoryCardViewModel> Categories { get; set; } = new List<CategoryCardViewModel>();
    public IEnumerable<EquipmentCardViewModel> FeaturedEquipment { get; set; } = new List<EquipmentCardViewModel>();
}

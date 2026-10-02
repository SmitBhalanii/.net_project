namespace RentKart.Web.ViewModels.Home;

using RentKart.Web.ViewModels.Equipment;
using RentKart.Web.ViewModels.Category;

public class HomePageViewModel
{
    public IEnumerable<CategoryCardViewModel> Categories { get; set; } = new List<CategoryCardViewModel>();
    public IEnumerable<EquipmentCardViewModel> FeaturedEquipment { get; set; } = new List<EquipmentCardViewModel>();
    public IEnumerable<RentKart.Web.ViewModels.Business.BusinessCardViewModel> PopularBusinesses { get; set; } = new List<RentKart.Web.ViewModels.Business.BusinessCardViewModel>();
}

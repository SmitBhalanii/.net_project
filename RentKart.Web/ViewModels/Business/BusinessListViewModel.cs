using System.Collections.Generic;

namespace RentKart.Web.ViewModels.Business;

public class BusinessListViewModel
{
    public IEnumerable<BusinessCardViewModel> Businesses { get; set; } = new List<BusinessCardViewModel>();
    public string? SearchTerm { get; set; }
    public string? City { get; set; }
    public IEnumerable<string> AvailableCities { get; set; } = new List<string>();
}

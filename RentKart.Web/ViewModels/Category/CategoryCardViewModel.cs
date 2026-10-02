namespace RentKart.Web.ViewModels.Category;

public class CategoryCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public int ListingCount { get; set; }
}

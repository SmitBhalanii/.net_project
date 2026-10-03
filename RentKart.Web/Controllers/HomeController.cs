using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentKart.Web.ViewModels;

using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

public class HomeController : Controller
{
    private readonly IBusinessService _businessService;
    private readonly ICategoryService _categoryService;
    private readonly IEquipmentSearchService _searchService;

    public HomeController(
        IBusinessService businessService,
        ICategoryService categoryService,
        IEquipmentSearchService searchService)
    {
        _businessService = businessService;
        _categoryService = categoryService;
        _searchService = searchService;
    }

    public async Task<IActionResult> Index()
    {
        var popularBusinesses = await _businessService.GetApprovedBusinessesAsync();
        var activeCategories = await _categoryService.GetActiveCategoriesAsync();
        var (recentEquipment, _) = await _searchService.SearchEquipmentAsync(
            null, null, null, null, null, null, null, null, null, "newest", 1, 4);

        var model = new RentKart.Web.ViewModels.Home.HomePageViewModel
        {
            Categories = activeCategories.Select(c => new RentKart.Web.ViewModels.Category.CategoryCardViewModel
            {
                Id = c.Id,
                Name = c.Name,
                IconClass = string.IsNullOrEmpty(c.ImagePath) ? "bi-tags" : c.ImagePath, // Fallback icon if no image
                ListingCount = c.Equipment?.Count ?? 0
            }).ToList(),
            FeaturedEquipment = recentEquipment.Select(e => new RentKart.Web.ViewModels.Equipment.EquipmentCardViewModel
            {
                Id = e.Id,
                Title = e.Name,
                BusinessName = e.Business?.BusinessName ?? "",
                Location = e.City ?? e.Business?.City ?? "",
                Price = e.RentalPrice,
                PriceUnit = e.RentalPeriod.ToString().ToLower(),
                Rating = e.Reviews.Any(r => r.Status == RentKart.Core.Enums.ReviewStatus.Published) 
                         ? (double)e.Reviews.Where(r => r.Status == RentKart.Core.Enums.ReviewStatus.Published).Average(r => r.EquipmentRating) 
                         : 0,
                ReviewCount = e.Reviews.Count(r => r.Status == RentKart.Core.Enums.ReviewStatus.Published)
            }).ToList(),
            PopularBusinesses = popularBusinesses.Take(4).Select(b => new RentKart.Web.ViewModels.Business.BusinessCardViewModel
            {
                Id = b.Id,
                BusinessName = b.BusinessName,
                Description = b.Description,
                LogoPath = b.LogoPath,
                City = b.City,
                State = b.State,
                ApprovalStatus = b.ApprovalStatus,
                EquipmentCount = b.Equipment.Count
            }).ToList()
        };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

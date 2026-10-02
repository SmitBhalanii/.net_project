using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentKart.Web.ViewModels;

using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

public class HomeController : Controller
{
    private readonly IBusinessService _businessService;

    public HomeController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    public async Task<IActionResult> Index()
    {
        var popularBusinesses = await _businessService.GetApprovedBusinessesAsync();
        
        var model = new RentKart.Web.ViewModels.Home.HomePageViewModel
        {
            Categories = new List<RentKart.Web.ViewModels.Category.CategoryCardViewModel>
            {
                new() { Id = 1, Name = "Photography", IconClass = "bi-camera", ListingCount = 42 },
                new() { Id = 2, Name = "Power Tools", IconClass = "bi-tools", ListingCount = 18 },
                new() { Id = 3, Name = "Event Gear", IconClass = "bi-speaker", ListingCount = 27 },
                new() { Id = 4, Name = "Camping", IconClass = "bi-tree", ListingCount = 15 }
            },
            FeaturedEquipment = new List<RentKart.Web.ViewModels.Equipment.EquipmentCardViewModel>
            {
                new() { Id = 1, Title = "Sony A7III Camera", BusinessName = "Lens Rentals", Location = "Vadodara", Price = 1500, Rating = 4.8, ReviewCount = 24 },
                new() { Id = 2, Title = "Bosch Power Drill", BusinessName = "Tool Hub", Location = "Nadiad", Price = 300, Rating = 4.5, ReviewCount = 12 },
                new() { Id = 3, Title = "JBL PartyBox 310", BusinessName = "Sound & Light", Location = "Vadodara", Price = 800, Rating = 4.9, ReviewCount = 56 },
                new() { Id = 4, Title = "4-Person Camping Tent", BusinessName = "Adventure Gear", Location = "Ahmedabad", Price = 500, Rating = 4.7, ReviewCount = 8 }
            },
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

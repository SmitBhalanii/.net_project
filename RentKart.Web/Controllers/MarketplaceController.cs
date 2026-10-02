using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[AllowAnonymous]
public class MarketplaceController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly ICategoryService _categoryService;

    public MarketplaceController(IEquipmentService equipmentService, ICategoryService categoryService)
    {
        _equipmentService = equipmentService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, string? city)
    {
        var equipment = await _equipmentService.SearchActiveEquipmentAsync(searchTerm, categoryId, city);
        ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
        ViewBag.SearchTerm = searchTerm;
        ViewBag.CategoryId = categoryId;
        ViewBag.City = city;
        
        return View(equipment);
    }

    public async Task<IActionResult> Details(int id)
    {
        var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
        
        if (equipment == null || !equipment.IsActive || !equipment.Category.IsActive || !equipment.Business.IsActive)
        {
            return NotFound();
        }

        return View(equipment);
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminCategoryController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IAuditLogService _auditLogService;

    public AdminCategoryController(ICategoryService categoryService, IAuditLogService auditLogService)
    {
        _categoryService = categoryService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            await _categoryService.CreateCategoryAsync(category);
            await _auditLogService.LogActionAsync(User.Identity?.Name, "CreateCategory", "Category", category.Id.ToString(), $"Category {category.Name} created.");
            TempData["SuccessMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            await _categoryService.UpdateCategoryAsync(category);
            await _auditLogService.LogActionAsync(User.Identity?.Name, "UpdateCategory", "Category", category.Id.ToString(), $"Category {category.Name} updated.");
            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }
}

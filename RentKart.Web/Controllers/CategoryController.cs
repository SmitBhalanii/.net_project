using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;
using RentKart.Web.Models.ViewModels;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View(new CategoryViewModel { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (await _categoryService.CategoryNameExistsAsync(model.Name))
            {
                ModelState.AddModelError("Name", "A category with this name already exists.");
                return View(model);
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                IsActive = model.IsActive
            };

            await _categoryService.CreateCategoryAsync(category);
            TempData["SuccessMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();

        var model = new CategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            if (await _categoryService.CategoryNameExistsAsync(model.Name, model.Id))
            {
                ModelState.AddModelError("Name", "A category with this name already exists.");
                return View(model);
            }

            var category = await _categoryService.GetCategoryByIdAsync(model.Id);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.Description = model.Description;
            category.IsActive = model.IsActive;

            await _categoryService.UpdateCategoryAsync(category);
            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }
}

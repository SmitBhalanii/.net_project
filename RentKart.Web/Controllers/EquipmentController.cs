using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Constants;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;
using RentKart.Infrastructure.Data;
using RentKart.Web.Models.ViewModels;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Business)]
public class EquipmentController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly ICategoryService _categoryService;
    private readonly IFileService _fileService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public EquipmentController(
        IEquipmentService equipmentService,
        ICategoryService categoryService,
        IFileService fileService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _equipmentService = equipmentService;
        _categoryService = categoryService;
        _fileService = fileService;
        _userManager = userManager;
        _context = context;
    }

    private async Task<int?> GetBusinessIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.UserId == user.Id);
        return business?.Id;
    }

    public async Task<IActionResult> Index()
    {
        var businessId = await GetBusinessIdAsync();
        if (businessId == null) return RedirectToAction("Create", "BusinessDashboard"); // Or show an error

        var equipment = await _equipmentService.GetEquipmentByBusinessAsync(businessId.Value);
        return View(equipment);
    }

    public async Task<IActionResult> Create()
    {
        var categories = await _categoryService.GetActiveCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(new EquipmentViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EquipmentViewModel viewModel)
    {
        var businessId = await GetBusinessIdAsync();
        if (businessId == null) return RedirectToAction("Index");

        if (ModelState.IsValid)
        {
            var equipment = new Equipment
            {
                EquipmentCode = $"EQP-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                BusinessId = businessId.Value,
                CategoryId = viewModel.CategoryId,
                Name = viewModel.Name,
                Brand = viewModel.Brand,
                Model = viewModel.Model,
                Description = viewModel.Description,
                RentalPrice = viewModel.RentalPrice,
                RentalPeriod = viewModel.RentalPeriod,
                SecurityDeposit = viewModel.SecurityDeposit,
                Quantity = viewModel.Quantity,
                Condition = viewModel.Condition,
                Status = viewModel.Status,
                IsActive = viewModel.IsActive,
                City = viewModel.City,
                State = viewModel.State,
                PostalCode = viewModel.PostalCode
            };

            var images = new List<EquipmentImage>();
            if (viewModel.Images != null)
            {
                foreach (var file in viewModel.Images)
                {
                    if (file.Length > 0)
                    {
                        var imagePath = await _fileService.SaveFileAsync(file.OpenReadStream(), file.FileName, "equipment");
                        images.Add(new EquipmentImage
                        {
                            ImagePath = imagePath,
                            IsPrimary = images.Count == 0 // Make first image primary
                        });
                    }
                }
            }

            await _equipmentService.CreateEquipmentAsync(equipment, images);
            TempData["SuccessMessage"] = "Equipment added successfully.";
            return RedirectToAction(nameof(Index));
        }

        var categories = await _categoryService.GetActiveCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(viewModel);
    }

    [AllowAnonymous]
    public IActionResult QrImage(string code, [FromServices] IQrCodeService qrCodeService)
    {
        var qrBytes = qrCodeService.GenerateQrCode($"/Equipment/Scan/{code}");
        return File(qrBytes, "image/png");
    }

    [AllowAnonymous]
    [Route("Equipment/Scan/{code}")]
    public async Task<IActionResult> Scan(string code, [FromServices] IRentalService rentalService)
    {
        var equipment = await rentalService.GetEquipmentByCodeAsync(code);
        if (equipment == null) return NotFound("Equipment not found.");
        return View(equipment);
    }
}

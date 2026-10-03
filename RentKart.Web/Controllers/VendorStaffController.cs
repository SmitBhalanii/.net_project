using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Constants;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;
using RentKart.Web.Models.ViewModels;

namespace RentKart.Web.Controllers;

[Authorize(Roles = RoleNames.Business)]
public class VendorStaffController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBusinessService _businessService;

    public VendorStaffController(UserManager<ApplicationUser> userManager, IBusinessService businessService)
    {
        _userManager = userManager;
        _businessService = businessService;
    }

    private async Task<int?> GetBusinessIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        
        var business = await _businessService.GetBusinessByUserIdAsync(user.Id);
        return business?.Id;
    }

    public async Task<IActionResult> Index()
    {
        var businessId = await GetBusinessIdAsync();
        if (businessId == null) return RedirectToAction("Index", "BusinessDashboard");

        var staffMembers = await _userManager.Users
            .Where(u => u.BusinessId == businessId)
            .ToListAsync();

        return View(staffMembers);
    }

    public IActionResult Create()
    {
        return View(new StaffViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffViewModel model)
    {
        var businessId = await GetBusinessIdAsync();
        if (businessId == null) return RedirectToAction("Index", "BusinessDashboard");

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsActive = model.IsActive,
                BusinessId = businessId.Value,
                CreatedAt = System.DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleNames.VendorStaff);
                TempData["SuccessMessage"] = "Staff member created successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var businessId = await GetBusinessIdAsync();
        if (businessId == null) return RedirectToAction("Index", "BusinessDashboard");

        var staff = await _userManager.FindByIdAsync(id);
        if (staff == null || staff.BusinessId != businessId) return NotFound();

        staff.IsActive = !staff.IsActive;
        await _userManager.UpdateAsync(staff);
        
        TempData["SuccessMessage"] = staff.IsActive ? "Staff member activated." : "Staff member deactivated.";
        return RedirectToAction(nameof(Index));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Constants;
using RentKart.Core.Entities;
using RentKart.Core.Enums;
using RentKart.Infrastructure.Data;
using RentKart.Web.ViewModels;

namespace RentKart.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult RegisterCustomer()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterCustomer(CustomerRegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DisplayName = $"{model.FirstName} {model.LastName}",
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleNames.Customer);
                _logger.LogInformation("Customer registered.");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "CustomerDashboard");
            }
            AddErrors(result);
        }
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult RegisterBusiness()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterBusiness(BusinessRegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.OwnerName,
                    LastName = "",
                    DisplayName = model.OwnerName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, RoleNames.Business);

                    var business = new Business
                    {
                        UserId = user.Id,
                        BusinessName = model.BusinessName,
                        OwnerName = model.OwnerName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        City = model.City,
                        State = model.State,
                        PostalCode = model.PostalCode,
                        Description = model.Description,
                        ApprovalStatus = BusinessApprovalStatus.Pending,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.Businesses.Add(business);
                    await _context.SaveChangesAsync();
                    
                    await transaction.CommitAsync();

                    _logger.LogInformation("Business registered.");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "BusinessDashboard");
                }
                
                AddErrors(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error registering business");
                ModelState.AddModelError(string.Empty, "An error occurred while creating your business account.");
            }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    #region Helpers

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            if (User.IsInRole(RoleNames.Admin)) return RedirectToAction("Index", "AdminDashboard");
            if (User.IsInRole(RoleNames.Business)) return RedirectToAction("Index", "BusinessDashboard");
            if (User.IsInRole(RoleNames.Customer)) return RedirectToAction("Index", "CustomerDashboard");
            
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }

    #endregion
}

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Controllers;

[Authorize(Roles = "Customer")]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;
    private readonly IEquipmentService _equipmentService;

    public WishlistController(IWishlistService wishlistService, IEquipmentService equipmentService)
    {
        _wishlistService = wishlistService;
        _equipmentService = equipmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var wishlist = await _wishlistService.GetCustomerWishlistAsync(userId);
        return View(wishlist);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int equipmentId, string returnUrl)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var equipment = await _equipmentService.GetEquipmentByIdAsync(equipmentId);
        if (equipment == null) return NotFound();

        var success = await _wishlistService.AddAsync(userId, equipmentId);
        if (success)
        {
            TempData["SuccessMessage"] = "Equipment added to your wishlist.";
        }
        else
        {
            TempData["ErrorMessage"] = "Equipment is already in your wishlist.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Details", "Equipment", new { id = equipmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int equipmentId, string returnUrl)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var success = await _wishlistService.RemoveAsync(userId, equipmentId);
        if (success)
        {
            TempData["SuccessMessage"] = "Equipment removed from your wishlist.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }
}

using System.Security.Claims;
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
    private readonly IReviewService _reviewService;
    private readonly IWishlistService _wishlistService;

    public MarketplaceController(
        IEquipmentService equipmentService, 
        ICategoryService categoryService,
        IReviewService reviewService,
        IWishlistService wishlistService)
    {
        _equipmentService = equipmentService;
        _categoryService = categoryService;
        _reviewService = reviewService;
        _wishlistService = wishlistService;
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

        var reviews = await _reviewService.GetEquipmentReviewsAsync(id);
        var publishedReviews = reviews.Where(r => r.Status == RentKart.Core.Enums.ReviewStatus.Published).ToList();
        
        ViewBag.Reviews = publishedReviews;
        ViewBag.AverageRating = publishedReviews.Any() ? publishedReviews.Average(r => r.EquipmentRating) : 0;
        ViewBag.ReviewCount = publishedReviews.Count;
        
        // Rating Distribution (5, 4, 3, 2, 1)
        var ratingDistribution = new Dictionary<int, int>();
        for (int i = 5; i >= 1; i--)
        {
            ratingDistribution[i] = publishedReviews.Count(r => r.EquipmentRating == i);
        }
        ViewBag.RatingDistribution = ratingDistribution;

        bool isInWishlist = false;
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Customer"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                isInWishlist = await _wishlistService.IsInWishlistAsync(userId, id);
            }
        }
        ViewBag.IsInWishlist = isInWishlist;

        return View(equipment);
    }
}

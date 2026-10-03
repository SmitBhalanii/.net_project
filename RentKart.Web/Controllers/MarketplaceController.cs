using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentKart.Core.Interfaces;
using RentKart.Web.ViewModels.Marketplace;
using System.Collections.Generic;

namespace RentKart.Web.Controllers;

[AllowAnonymous]
public class MarketplaceController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly ICategoryService _categoryService;
    private readonly IReviewService _reviewService;
    private readonly IWishlistService _wishlistService;
    private readonly IEquipmentSearchService _searchService;
    private readonly IBusinessService _businessService;

    public MarketplaceController(
        IEquipmentService equipmentService, 
        ICategoryService categoryService,
        IReviewService reviewService,
        IWishlistService wishlistService,
        IEquipmentSearchService searchService,
        IBusinessService businessService)
    {
        _equipmentService = equipmentService;
        _categoryService = categoryService;
        _reviewService = reviewService;
        _wishlistService = wishlistService;
        _searchService = searchService;
        _businessService = businessService;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Search");
    }

    [HttpGet]
    public async Task<IActionResult> Search(MarketplaceSearchViewModel model)
    {
        // Default values
        if (model.PageSize <= 0) model.PageSize = 12;
        if (model.CurrentPage <= 0) model.CurrentPage = 1;

        var categories = await _categoryService.GetActiveCategoriesAsync();
        model.Categories = categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name,
            Selected = model.CategoryId == c.Id
        });

        // Search database
        var (items, totalCount) = await _searchService.SearchEquipmentAsync(
            model.SearchTerm,
            model.CategoryId,
            model.Location,
            model.MinPrice,
            model.MaxPrice,
            model.MinRating,
            model.BusinessId,
            model.StartDate,
            model.EndDate,
            model.SortBy,
            model.CurrentPage,
            model.PageSize);

        model.Results = items.Select(e => new RentKart.Web.ViewModels.Equipment.EquipmentCardViewModel
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
        }).ToList();
        
        model.TotalItems = totalCount;

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
        
        if (equipment == null || !equipment.IsActive || !equipment.Category.IsActive || !equipment.Business.IsActive || equipment.Business.ApprovalStatus != RentKart.Core.Enums.BusinessApprovalStatus.Approved)
        {
            return NotFound();
        }

        var reviews = await _reviewService.GetEquipmentReviewsAsync(id);
        var publishedReviews = reviews.Where(r => r.Status == RentKart.Core.Enums.ReviewStatus.Published).ToList();
        
        ViewBag.Reviews = publishedReviews;
        ViewBag.AverageRating = publishedReviews.Any() ? publishedReviews.Average(r => (double)r.EquipmentRating) : 0;
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

        // Related Equipment
        var related = await _equipmentService.SearchActiveEquipmentAsync(null, equipment.CategoryId, null);
        ViewBag.RelatedEquipment = related.Where(e => e.Id != id && e.Business.ApprovalStatus == RentKart.Core.Enums.BusinessApprovalStatus.Approved).Take(4).ToList();

        return View(equipment);
    }

    public async Task<IActionResult> Business(int id)
    {
        var business = await _businessService.GetBusinessByIdAsync(id);

        if (business == null || !business.IsActive || business.ApprovalStatus != RentKart.Core.Enums.BusinessApprovalStatus.Approved)
        {
            return NotFound();
        }

        return View(business);
    }
}

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;
using RentKart.Web.ViewModels.Business;
using RentKart.Web.ViewModels.Equipment;

namespace RentKart.Web.Controllers;

public class BusinessController : Controller
{
    private readonly IBusinessService _businessService;
    private readonly IFileService _fileService;
    private readonly IReviewService _reviewService;

    public BusinessController(IBusinessService businessService, IFileService fileService, IReviewService reviewService)
    {
        _businessService = businessService;
        _fileService = fileService;
        _reviewService = reviewService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchTerm, string? city)
    {
        var businesses = await _businessService.GetApprovedBusinessesAsync(searchTerm, city);
        var cities = await _businessService.GetAvailableCitiesAsync();

        var viewModel = new BusinessListViewModel
        {
            SearchTerm = searchTerm,
            City = city,
            AvailableCities = cities,
            Businesses = businesses.Select(b => new BusinessCardViewModel
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

        return View(viewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var business = await _businessService.GetBusinessProfileAsync(id);
        if (business == null || (!User.IsInRole("Admin") && business.ApprovalStatus != RentKart.Core.Enums.BusinessApprovalStatus.Approved))
        {
            return NotFound();
        }

        var viewModel = new BusinessProfileViewModel
        {
            BusinessId = business.Id,
            BusinessName = business.BusinessName,
            Description = business.Description,
            LogoPath = business.LogoPath,
            CoverImagePath = business.CoverImagePath,
            City = business.City,
            State = business.State,
            PostalCode = business.PostalCode,
            Website = business.Website,
            ContactEmail = business.Email,
            ContactPhone = business.PhoneNumber,
            ApprovalStatus = business.ApprovalStatus,
            EquipmentCount = business.Equipment.Count,
            Equipment = business.Equipment.Select(e => new EquipmentCardViewModel
            {
                Id = e.Id,
                Title = e.Name,
                BusinessName = business.BusinessName,
                Price = e.RentalPrice,
                PriceUnit = e.RentalPeriod.ToString(),
                Location = $"{e.City}, {e.State}",
                ImageUrl = e.EquipmentImages.FirstOrDefault(i => i.IsPrimary)?.ImagePath ?? e.EquipmentImages.FirstOrDefault()?.ImagePath ?? "",
                Rating = 0,
                ReviewCount = 0
            }).ToList()
        };

        var reviews = await _reviewService.GetBusinessReviewsAsync(id);
        var publishedReviews = reviews.Where(r => r.Status == RentKart.Core.Enums.ReviewStatus.Published).ToList();
        
        ViewBag.Reviews = publishedReviews;
        ViewBag.AverageRating = publishedReviews.Any() ? publishedReviews.Average(r => r.BusinessRating) : 0;
        ViewBag.ReviewCount = publishedReviews.Count;
        
        var ratingDistribution = new System.Collections.Generic.Dictionary<int, int>();
        for (int i = 5; i >= 1; i--)
        {
            ratingDistribution[i] = publishedReviews.Count(r => r.BusinessRating == i);
        }
        ViewBag.RatingDistribution = ratingDistribution;

        return View(viewModel);
    }

    [Authorize(Roles = "Business")]
    public async Task<IActionResult> MyBusiness()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var business = await _businessService.GetBusinessByUserIdAsync(userId);
        if (business == null) return NotFound();

        return RedirectToAction(nameof(Details), new { id = business.Id });
    }

    [Authorize(Roles = "Business")]
    public async Task<IActionResult> Edit()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var business = await _businessService.GetBusinessByUserIdAsync(userId);
        if (business == null) return NotFound();

        var viewModel = new BusinessEditViewModel
        {
            Id = business.Id,
            BusinessName = business.BusinessName,
            Description = business.Description,
            Address = business.Address,
            City = business.City,
            State = business.State,
            PostalCode = business.PostalCode,
            ContactEmail = business.Email,
            ContactPhone = business.PhoneNumber,
            Website = business.Website,
            LogoPath = business.LogoPath
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Business")]
    public async Task<IActionResult> Edit(BusinessEditViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var business = await _businessService.GetBusinessByUserIdAsync(userId);
        if (business == null || business.Id != model.Id)
        {
            return Forbid(); // Ensure they are editing their own business
        }

        if (ModelState.IsValid)
        {
            business.BusinessName = model.BusinessName;
            business.Description = model.Description;
            business.Address = model.Address;
            business.City = model.City;
            business.State = model.State;
            business.PostalCode = model.PostalCode;
            business.Email = model.ContactEmail;
            business.PhoneNumber = model.ContactPhone;
            business.Website = model.Website;

            if (model.LogoFile != null)
            {
                // Delete old logo if exists
                if (!string.IsNullOrEmpty(business.LogoPath))
                {
                    _fileService.DeleteFile(business.LogoPath);
                }

                business.LogoPath = await _fileService.SaveFileAsync(model.LogoFile.OpenReadStream(), model.LogoFile.FileName, "business_logos");
            }

            await _businessService.UpdateBusinessProfileAsync(business);
            TempData["SuccessMessage"] = "Business profile updated successfully.";
            return RedirectToAction(nameof(MyBusiness));
        }

        return View(model);
    }
}

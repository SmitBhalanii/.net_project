using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentKart.Core.Entities;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;
using RentKart.Web.ViewModels.Review;

namespace RentKart.Web.Controllers;

[Authorize]
public class ReviewController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly IRentalService _rentalService;

    public ReviewController(IReviewService reviewService, IRentalService rentalService)
    {
        _reviewService = reviewService;
        _rentalService = rentalService;
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create(int rentalId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var canReview = await _reviewService.CanCustomerReviewAsync(userId, rentalId);
        if (!canReview)
        {
            TempData["ErrorMessage"] = "You are not eligible to review this rental. It must be completed and not previously reviewed.";
            return RedirectToAction("MyRentals", "Rental");
        }

        var rental = await _rentalService.GetRentalByIdAsync(rentalId);
        if (rental == null) return NotFound();

        var vm = new CreateReviewViewModel
        {
            RentalId = rentalId,
            EquipmentId = rental.EquipmentId,
            BusinessId = rental.Booking.Equipment.BusinessId,
            BookingId = rental.BookingId,
            EquipmentName = rental.Equipment.Name,
            BusinessName = rental.Booking.Equipment.Business.BusinessName
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var canReview = await _reviewService.CanCustomerReviewAsync(userId, model.RentalId);
        if (!canReview)
        {
            TempData["ErrorMessage"] = "You are not eligible to review this rental.";
            return RedirectToAction("MyRentals", "Rental");
        }

        var review = new Review
        {
            CustomerId = userId,
            RentalId = model.RentalId,
            BookingId = model.BookingId,
            EquipmentId = model.EquipmentId,
            BusinessId = model.BusinessId,
            EquipmentRating = model.EquipmentRating,
            BusinessRating = model.BusinessRating,
            Title = model.Title,
            Comment = model.Comment
        };

        await _reviewService.CreateReviewAsync(review);

        TempData["SuccessMessage"] = "Your review was submitted successfully and is pending moderation.";
        return RedirectToAction("MyReviews");
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MyReviews()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var reviews = await _reviewService.GetCustomerReviewsAsync(userId);
        return View(reviews);
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null || review.CustomerId != userId) return NotFound();

        var vm = new EditReviewViewModel
        {
            Id = review.Id,
            EquipmentRating = review.EquipmentRating,
            BusinessRating = review.BusinessRating,
            Title = review.Title,
            Comment = review.Comment,
            EquipmentName = review.Equipment.Name,
            BusinessName = review.Business.BusinessName
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditReviewViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var review = await _reviewService.GetReviewByIdAsync(model.Id);
        if (review == null || review.CustomerId != userId) return NotFound();

        review.EquipmentRating = model.EquipmentRating;
        review.BusinessRating = model.BusinessRating;
        review.Title = model.Title;
        review.Comment = model.Comment;

        await _reviewService.UpdateReviewAsync(review);

        TempData["SuccessMessage"] = "Review updated successfully. It is now pending moderation.";
        return RedirectToAction(nameof(MyReviews));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Pending()
    {
        var pendingReviews = await _reviewService.GetPendingReviewsAsync();
        return View(pendingReviews);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Moderate(int reviewId, ReviewStatus status)
    {
        var success = await _reviewService.ModerateReviewAsync(reviewId, status);
        if (success)
        {
            TempData["SuccessMessage"] = $"Review marked as {status}.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to moderate review.";
        }

        return RedirectToAction(nameof(Pending));
    }
}

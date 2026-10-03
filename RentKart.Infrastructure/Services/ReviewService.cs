using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;
using RentKart.Infrastructure.Data;

namespace RentKart.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public ReviewService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<bool> CanCustomerReviewAsync(string customerId, int rentalId)
    {
        var rental = await _context.Rentals
            .Include(r => r.Booking)
            .FirstOrDefaultAsync(r => r.Id == rentalId && r.Booking.CustomerId == customerId);

        if (rental == null || rental.Status != RentalStatus.Completed)
        {
            return false;
        }

        var existingReview = await _context.Reviews.AnyAsync(r => r.RentalId == rentalId);
        return !existingReview;
    }

    public async Task<Review> CreateReviewAsync(Review review)
    {
        review.CreatedAt = DateTime.UtcNow;
        review.Status = ReviewStatus.Pending; // Default to pending for moderation

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        
        await _notificationService.CreateNotificationAsync(
            review.CustomerId,
            NotificationType.ReviewSubmitted,
            "Review Submitted",
            "Your review has been submitted successfully. It will appear after approval.",
            "Review",
            review.Id.ToString());
            
        return review;
    }

    public async Task<Review?> GetReviewByRentalAsync(int rentalId)
    {
        return await _context.Reviews
            .Include(r => r.Equipment)
            .Include(r => r.Business)
            .FirstOrDefaultAsync(r => r.RentalId == rentalId);
    }

    public async Task<Review?> GetReviewByIdAsync(int reviewId)
    {
        return await _context.Reviews
            .Include(r => r.Equipment)
            .Include(r => r.Business)
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == reviewId);
    }

    public async Task<IEnumerable<Review>> GetEquipmentReviewsAsync(int equipmentId)
    {
        return await _context.Reviews
            .Include(r => r.Customer)
            .Where(r => r.EquipmentId == equipmentId && r.Status == ReviewStatus.Published)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetBusinessReviewsAsync(int businessId)
    {
        return await _context.Reviews
            .Include(r => r.Customer)
            .Include(r => r.Equipment)
            .Where(r => r.BusinessId == businessId && r.Status == ReviewStatus.Published)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetCustomerReviewsAsync(string customerId)
    {
        return await _context.Reviews
            .Include(r => r.Equipment)
            .Include(r => r.Business)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
    {
        return await _context.Reviews
            .Include(r => r.Customer)
            .Include(r => r.Equipment)
            .Include(r => r.Business)
            .Where(r => r.Status == ReviewStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review> UpdateReviewAsync(Review review)
    {
        review.UpdatedAt = DateTime.UtcNow;
        // Upon update, usually reviews go back to Pending for moderation
        review.Status = ReviewStatus.Pending;
        
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<bool> ModerateReviewAsync(int reviewId, ReviewStatus status)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null) return false;

        review.Status = status;
        review.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        var reviewWithIncludes = await _context.Reviews.Include(r => r.Equipment).FirstOrDefaultAsync(r => r.Id == reviewId);
        if (reviewWithIncludes != null)
        {
            if (status == ReviewStatus.Published)
            {
                await _notificationService.CreateNotificationAsync(
                    reviewWithIncludes.CustomerId,
                    NotificationType.ReviewApproved,
                    "Review Published",
                    $"Your review for {reviewWithIncludes.Equipment.Name} has been published.",
                    "Review",
                    reviewWithIncludes.Id.ToString());
            }
            else if (status == ReviewStatus.Rejected)
            {
                await _notificationService.CreateNotificationAsync(
                    reviewWithIncludes.CustomerId,
                    NotificationType.ReviewRejected,
                    "Review Not Published",
                    $"Your review for {reviewWithIncludes.Equipment.Name} was not published as it violated our guidelines.",
                    "Review",
                    reviewWithIncludes.Id.ToString());
            }
        }
        
        return true;
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;
using RentKart.Core.Enums;

namespace RentKart.Core.Interfaces;

public interface IReviewService
{
    Task<bool> CanCustomerReviewAsync(string customerId, int rentalId);
    Task<Review> CreateReviewAsync(Review review);
    Task<Review?> GetReviewByRentalAsync(int rentalId);
    Task<Review?> GetReviewByIdAsync(int reviewId);
    Task<IEnumerable<Review>> GetEquipmentReviewsAsync(int equipmentId);
    Task<IEnumerable<Review>> GetBusinessReviewsAsync(int businessId);
    Task<IEnumerable<Review>> GetCustomerReviewsAsync(string customerId);
    Task<IEnumerable<Review>> GetPendingReviewsAsync();
    Task<Review> UpdateReviewAsync(Review review);
    Task<bool> ModerateReviewAsync(int reviewId, ReviewStatus status);
}

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

public class EquipmentSearchService : IEquipmentSearchService
{
    private readonly ApplicationDbContext _context;

    public EquipmentSearchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Equipment> Items, int TotalCount)> SearchEquipmentAsync(
        string? searchTerm,
        int? categoryId,
        string? location,
        decimal? minPrice,
        decimal? maxPrice,
        int? minRating,
        int? businessId,
        DateTime? startDate,
        DateTime? endDate,
        string sortBy,
        int page,
        int pageSize)
    {
        var query = _context.Equipment
            .Include(e => e.Category)
            .Include(e => e.Business)
            .Include(e => e.Reviews)
            .AsNoTracking()
            .Where(e => e.IsActive && 
                        e.Business.ApprovalStatus == BusinessApprovalStatus.Approved && 
                        e.Business.IsActive && 
                        e.Category.IsActive);

        // Search Term
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e => 
                e.Name.ToLower().Contains(term) || 
                (e.Description != null && e.Description.ToLower().Contains(term)) ||
                e.Category.Name.ToLower().Contains(term) ||
                e.Business.BusinessName.ToLower().Contains(term));
        }

        // Category
        if (categoryId.HasValue && categoryId > 0)
        {
            query = query.Where(e => e.CategoryId == categoryId.Value);
        }

        // Business
        if (businessId.HasValue && businessId > 0)
        {
            query = query.Where(e => e.BusinessId == businessId.Value);
        }

        // Location
        if (!string.IsNullOrWhiteSpace(location))
        {
            var loc = location.ToLower();
            query = query.Where(e => 
                (e.City != null && e.City.ToLower().Contains(loc)) ||
                (e.Business.City != null && e.Business.City.ToLower().Contains(loc)) ||
                (e.PostalCode != null && e.PostalCode.ToLower().Contains(loc)) ||
                (e.Business.PostalCode != null && e.Business.PostalCode.ToLower().Contains(loc)));
        }

        // Price
        if (minPrice.HasValue && minPrice >= 0)
        {
            query = query.Where(e => e.RentalPrice >= minPrice.Value);
        }
        if (maxPrice.HasValue && maxPrice >= 0)
        {
            query = query.Where(e => e.RentalPrice <= maxPrice.Value);
        }

        // Rating
        if (minRating.HasValue && minRating > 0)
        {
            // Only consider published reviews
            query = query.Where(e => e.Reviews.Where(r => r.Status == ReviewStatus.Published).Any() && 
                                     e.Reviews.Where(r => r.Status == ReviewStatus.Published).Average(r => (double)r.EquipmentRating) >= minRating.Value);
        }

        // Availability (Dates)
        if (startDate.HasValue && endDate.HasValue)
        {
            var blockingStatuses = new[] { BookingStatus.Pending, BookingStatus.Approved, BookingStatus.Active };
            
            query = query.Where(e => !e.Bookings.Any(b => 
                blockingStatuses.Contains(b.Status) && 
                b.StartDate <= endDate.Value && 
                b.EndDate >= startDate.Value));
        }

        // Count
        var totalCount = await query.CountAsync();

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "price-low" => query.OrderBy(e => e.RentalPrice),
            "price-high" => query.OrderByDescending(e => e.RentalPrice),
            "rating" => query.OrderByDescending(e => e.Reviews.Where(r => r.Status == ReviewStatus.Published).Average(r => (double?)r.EquipmentRating) ?? 0),
            "newest" => query.OrderByDescending(e => e.CreatedAt),
            "popular" => query.OrderByDescending(e => e.Bookings.Count(b => b.Status == BookingStatus.Completed)),
            _ => query.OrderByDescending(e => e.CreatedAt) // Default fallback
        };

        // Pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

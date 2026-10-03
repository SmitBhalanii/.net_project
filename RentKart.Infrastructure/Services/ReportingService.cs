using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.DTOs.Reports;
using RentKart.Core.Entities;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;
using RentKart.Infrastructure.Data;

namespace RentKart.Infrastructure.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ApplicationDbContext _db;

        public ReportingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            // Total users with 'Customer' role? Identity doesn't store role natively in Users table easily without join
            // Let's count by Bookings/Businesses logic or just total users
            var totalUsers = await _db.Users.CountAsync();
            var totalBusinesses = await _db.Businesses.CountAsync();
            var pendingVendors = await _db.Businesses.CountAsync(b => b.ApprovalStatus == BusinessApprovalStatus.Pending);
            var approvedVendors = await _db.Businesses.CountAsync(b => b.ApprovalStatus == BusinessApprovalStatus.Approved);
            var suspendedVendors = await _db.Businesses.CountAsync(b => b.ApprovalStatus == BusinessApprovalStatus.Suspended);
            var totalCustomers = totalUsers - totalBusinesses; // Rough estimate since admin is included, but we can do a better query if needed
            
            var totalEquipment = await _db.Equipment.CountAsync();
            var totalBookings = await _db.Bookings.CountAsync(b => b.CreatedAt >= start && b.CreatedAt <= end);
            var activeRentals = await _db.Rentals.CountAsync(r => r.Status == RentalStatus.Active);

            // Revenue: successful payments for rentals
            var successfulPayments = _db.Payments
                .Where(p => p.PaymentStatus == PaymentStatus.Succeeded && p.CreatedAt >= start && p.CreatedAt <= end);
            
            var totalRevenue = await successfulPayments.SumAsync(p => p.Amount);

            // Revenue Trend (Last 12 months by default)
            var trendStart = DateTime.UtcNow.AddMonths(-11).Date;
            trendStart = new DateTime(trendStart.Year, trendStart.Month, 1);
            
            var twelveMonthPayments = await _db.Payments
                .Where(p => p.PaymentStatus == PaymentStatus.Succeeded && p.CreatedAt >= trendStart)
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .Select(g => new 
                { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Revenue = g.Sum(x => x.Amount) 
                })
                .ToListAsync();

            var revenueTrend = new List<RevenueTrendItemDto>();
            for (int i = 0; i < 12; i++)
            {
                var monthDate = trendStart.AddMonths(i);
                var revenue = twelveMonthPayments
                    .Where(p => p.Year == monthDate.Year && p.Month == monthDate.Month)
                    .Select(p => p.Revenue)
                    .FirstOrDefault();

                revenueTrend.Add(new RevenueTrendItemDto
                {
                    Period = monthDate.ToString("MMM yyyy"),
                    Revenue = revenue
                });
            }

            // Booking Status Distribution
            var bookingDistribution = await _db.Bookings
                .Where(b => b.CreatedAt >= start && b.CreatedAt <= end)
                .GroupBy(b => b.Status)
                .Select(g => new BookingStatusItemDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            // Top Equipment
            var topEquipment = await _db.Equipment
                .Include(e => e.Business)
                .Include(e => e.Category)
                .Select(e => new
                {
                    Equipment = e,
                    RentalCount = e.Bookings.Count(b => b.Status == BookingStatus.Completed),
                    Revenue = e.Bookings.SelectMany(b => b.Payments).Where(p => p.PaymentStatus == PaymentStatus.Succeeded).Sum(p => (decimal?)p.Amount) ?? 0,
                    AvgRating = e.Reviews.Where(r => r.Status == ReviewStatus.Published).Average(r => (double?)r.EquipmentRating) ?? 0
                })
                .OrderByDescending(x => x.RentalCount)
                .Take(10)
                .ToListAsync();

            var topEquipmentDto = topEquipment.Select((x, i) => new TopEquipmentItemDto
            {
                Rank = i + 1,
                EquipmentName = x.Equipment.Name,
                BusinessName = x.Equipment.Business.BusinessName,
                CategoryName = x.Equipment.Category.Name,
                RentalCount = x.RentalCount,
                Revenue = x.Revenue,
                AverageRating = x.AvgRating
            }).ToList();

            // Top Categories
            var topCategories = await _db.Categories
                .Select(c => new TopCategoryItemDto
                {
                    CategoryName = c.Name,
                    RentalCount = c.Equipment.SelectMany(e => e.Bookings).Count(b => b.Status == BookingStatus.Completed),
                    Revenue = c.Equipment.SelectMany(e => e.Bookings).SelectMany(b => b.Payments).Where(p => p.PaymentStatus == PaymentStatus.Succeeded).Sum(p => (decimal?)p.Amount) ?? 0
                })
                .OrderByDescending(x => x.RentalCount)
                .Take(5)
                .ToListAsync();

            // Top Businesses
            var topBusinesses = await _db.Businesses
                .Select(b => new
                {
                    Business = b,
                    CompletedRentals = b.Bookings.Count(bk => bk.Status == BookingStatus.Completed),
                    Revenue = b.Bookings.SelectMany(bk => bk.Payments).Where(p => p.PaymentStatus == PaymentStatus.Succeeded).Sum(p => (decimal?)p.Amount) ?? 0,
                    AvgRating = b.Reviews.Where(r => r.Status == ReviewStatus.Published).Average(r => (double?)r.BusinessRating) ?? 0,
                    ActiveEquip = b.Equipment.Count(e => e.IsActive)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            var topBusinessesDto = topBusinesses.Select((x, i) => new TopBusinessItemDto
            {
                Rank = i + 1,
                BusinessName = x.Business.BusinessName,
                CompletedRentals = x.CompletedRentals,
                Revenue = x.Revenue,
                AverageRating = x.AvgRating,
                ActiveEquipment = x.ActiveEquip,
                Bookings = x.Business.Bookings.Count
            }).ToList();

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalCustomers = totalCustomers,
                TotalBusinesses = totalBusinesses,
                PendingVendors = pendingVendors,
                ApprovedVendors = approvedVendors,
                SuspendedVendors = suspendedVendors,
                TotalEquipment = totalEquipment,
                TotalBookings = totalBookings,
                ActiveRentals = activeRentals,
                TotalRevenue = totalRevenue,
                StartDate = start,
                EndDate = end,
                RevenueTrend = revenueTrend,
                BookingStatusDistribution = bookingDistribution,
                TopEquipment = topEquipmentDto,
                TopCategories = topCategories,
                TopBusinesses = topBusinessesDto
            };
        }

        public async Task<BusinessDashboardDto> GetBusinessDashboardAsync(int businessId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var businessQuery = _db.Businesses.Where(b => b.Id == businessId);
            
            var totalEquipment = await _db.Equipment.CountAsync(e => e.BusinessId == businessId);
            var activeEquipment = await _db.Equipment.CountAsync(e => e.BusinessId == businessId && e.IsActive);
            var totalBookings = await _db.Bookings.CountAsync(b => b.BusinessId == businessId && b.CreatedAt >= start && b.CreatedAt <= end);
            var pendingBookings = await _db.Bookings.CountAsync(b => b.BusinessId == businessId && b.Status == BookingStatus.Pending);
            
            var activeRentals = await _db.Rentals.CountAsync(r => r.BusinessId == businessId && (r.Status == RentalStatus.Active));
            var completedRentals = await _db.Rentals.CountAsync(r => r.BusinessId == businessId && r.Status == RentalStatus.Completed);
            
            var successfulPayments = _db.Payments
                .Where(p => p.Booking.BusinessId == businessId && p.PaymentStatus == PaymentStatus.Succeeded && p.CreatedAt >= start && p.CreatedAt <= end);
            
            var totalRevenue = await successfulPayments.SumAsync(p => p.Amount);

            var avgRating = await _db.Reviews
                .Where(r => r.BusinessId == businessId && r.Status == ReviewStatus.Published)
                .AverageAsync(r => (double?)r.BusinessRating) ?? 0;
                
            var totalReviews = await _db.Reviews
                .CountAsync(r => r.BusinessId == businessId && r.Status == ReviewStatus.Published);

            // Revenue Trend (Last 12 months by default)
            var trendStart = DateTime.UtcNow.AddMonths(-11).Date;
            trendStart = new DateTime(trendStart.Year, trendStart.Month, 1);
            
            var twelveMonthPayments = await _db.Payments
                .Where(p => p.Booking.BusinessId == businessId && p.PaymentStatus == PaymentStatus.Succeeded && p.CreatedAt >= trendStart)
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .Select(g => new 
                { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Revenue = g.Sum(x => x.Amount) 
                })
                .ToListAsync();

            var revenueTrend = new List<RevenueTrendItemDto>();
            for (int i = 0; i < 12; i++)
            {
                var monthDate = trendStart.AddMonths(i);
                var revenue = twelveMonthPayments
                    .Where(p => p.Year == monthDate.Year && p.Month == monthDate.Month)
                    .Select(p => p.Revenue)
                    .FirstOrDefault();

                revenueTrend.Add(new RevenueTrendItemDto
                {
                    Period = monthDate.ToString("MMM yyyy"),
                    Revenue = revenue
                });
            }

            // Booking Status Distribution
            var bookingDistribution = await _db.Bookings
                .Where(b => b.BusinessId == businessId && b.CreatedAt >= start && b.CreatedAt <= end)
                .GroupBy(b => b.Status)
                .Select(g => new BookingStatusItemDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            // Equipment Performance
            var equipmentPerformance = await _db.Equipment
                .Where(e => e.BusinessId == businessId)
                .Select(e => new EquipmentPerformanceItemDto
                {
                    EquipmentName = e.Name,
                    Bookings = e.Bookings.Count,
                    CompletedRentals = e.Bookings.Count(b => b.Status == BookingStatus.Completed),
                    Revenue = e.Bookings.SelectMany(b => b.Payments).Where(p => p.PaymentStatus == PaymentStatus.Succeeded).Sum(p => (decimal?)p.Amount) ?? 0,
                    AverageRating = e.Reviews.Where(r => r.Status == ReviewStatus.Published).Average(r => (double?)r.EquipmentRating) ?? 0,
                    IsAvailable = e.IsActive
                })
                .OrderByDescending(x => x.CompletedRentals)
                .Take(10)
                .ToListAsync();

            return new BusinessDashboardDto
            {
                TotalEquipment = totalEquipment,
                ActiveEquipment = activeEquipment,
                TotalBookings = totalBookings,
                PendingBookings = pendingBookings,
                ActiveRentals = activeRentals,
                CompletedRentals = completedRentals,
                TotalRevenue = totalRevenue,
                AverageRating = avgRating,
                TotalReviews = totalReviews,
                StartDate = start,
                EndDate = end,
                RevenueTrend = revenueTrend,
                BookingStatusDistribution = bookingDistribution,
                EquipmentPerformance = equipmentPerformance
            };
        }

        public async Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(string customerId)
        {
            var totalBookings = await _db.Bookings.CountAsync(b => b.CustomerId == customerId);
            var activeRentals = await _db.Rentals.CountAsync(r => r.CustomerId == customerId && (r.Status == RentalStatus.Active));
            var completedRentals = await _db.Rentals.CountAsync(r => r.CustomerId == customerId && r.Status == RentalStatus.Completed);
            var cancelledBookings = await _db.Bookings.CountAsync(b => b.CustomerId == customerId && b.Status == BookingStatus.Cancelled);
            var wishlistItems = await _db.WishlistItems.CountAsync(w => w.CustomerId == customerId);
            
            var reviewsWritten = await _db.Reviews.CountAsync(r => r.CustomerId == customerId && r.Status == ReviewStatus.Published);
            var avgRatingGiven = await _db.Reviews
                .Where(r => r.CustomerId == customerId && r.Status == ReviewStatus.Published)
                .AverageAsync(r => (double?)((r.EquipmentRating + r.BusinessRating) / 2.0)) ?? 0;

            var totalSpent = await _db.Payments
                .Where(p => p.CustomerId == customerId && p.PaymentStatus == PaymentStatus.Succeeded)
                .SumAsync(p => p.Amount);

            // Monthly Rentals
            var trendStart = DateTime.UtcNow.AddMonths(-5).Date;
            trendStart = new DateTime(trendStart.Year, trendStart.Month, 1);

            var sixMonthRentals = await _db.Rentals
                .Where(r => r.CustomerId == customerId && r.CreatedAt >= trendStart)
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var monthlyRentals = new List<MonthlyRentalItemDto>();
            for (int i = 0; i < 6; i++)
            {
                var monthDate = trendStart.AddMonths(i);
                var count = sixMonthRentals
                    .Where(r => r.Year == monthDate.Year && r.Month == monthDate.Month)
                    .Select(r => r.Count)
                    .FirstOrDefault();

                monthlyRentals.Add(new MonthlyRentalItemDto
                {
                    Month = monthDate.ToString("MMM yyyy"),
                    Count = count
                });
            }

            // Top Categories
            var topCategories = await _db.Rentals
                .Where(r => r.CustomerId == customerId)
                .GroupBy(r => r.Equipment.Category.Name)
                .Select(g => new TopCategoryItemDto
                {
                    CategoryName = g.Key,
                    RentalCount = g.Count(),
                    Revenue = 0 // Not relevant for customer spending breakdown usually, or we can calculate it
                })
                .OrderByDescending(x => x.RentalCount)
                .Take(5)
                .ToListAsync();

            return new CustomerStatisticsDto
            {
                TotalBookings = totalBookings,
                ActiveRentals = activeRentals,
                CompletedRentals = completedRentals,
                CancelledBookings = cancelledBookings,
                WishlistItems = wishlistItems,
                ReviewsWritten = reviewsWritten,
                TotalSpent = totalSpent,
                AverageRatingGiven = avgRatingGiven,
                MonthlyRentals = monthlyRentals,
                TopCategories = topCategories
            };
        }
    }
}

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

            var totalUsers = await _db.Users.CountAsync();
            var totalBusinesses = await _db.Businesses.CountAsync();
            var pendingVendors = await _db.Businesses.CountAsync(b => b.ApprovalStatus == BusinessApprovalStatus.Pending);
            var approvedVendors = await _db.Businesses.CountAsync(b => b.ApprovalStatus == BusinessApprovalStatus.Approved);
            var suspendedVendors = await _db.Businesses.CountAsync(b => b.ApprovalStatus == BusinessApprovalStatus.Suspended);
            var totalCustomers = totalUsers - totalBusinesses; 
            
            var equipment = await _db.Equipment.AsNoTracking().ToListAsync();
            var totalEquipment = equipment.Count;
            var availableEquipment = equipment.Count(e => e.Status == EquipmentStatus.Active);
            var rentedEquipment = equipment.Count(e => e.Status == EquipmentStatus.Rented);
            var maintenanceEquipment = equipment.Count(e => e.Status == EquipmentStatus.UnderMaintenance);
            var inactiveEquipment = equipment.Count(e => e.Status == EquipmentStatus.Inactive);

            var totalBookings = await _db.Bookings.CountAsync(b => b.CreatedAt >= start && b.CreatedAt <= end);
            var pendingBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Pending && b.CreatedAt >= start && b.CreatedAt <= end);
            var confirmedBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed && b.CreatedAt >= start && b.CreatedAt <= end);
            var cancelledBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled && b.CreatedAt >= start && b.CreatedAt <= end);
            var completedBookingsCount = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Completed && b.CreatedAt >= start && b.CreatedAt <= end);

            var activeRentals = await _db.Rentals.CountAsync(r => r.Status == RentalStatus.Active);
            var completedRentals = await _db.Rentals.CountAsync(r => r.Status == RentalStatus.Completed);
            var overdueRentals = await _db.Rentals.CountAsync(r => r.Status == RentalStatus.Active && r.ExpectedReturnDate < DateTime.UtcNow);
            var returnedRentals = await _db.Rentals.CountAsync(r => r.Status == RentalStatus.Returned);

            // Revenue calculation using only RentalAmount. Ignore SecurityDeposit.
            var completedBookings = await _db.Bookings.AsNoTracking()
                .Where(b => (b.PaymentStatus == PaymentStatus.Succeeded || b.PaymentStatus == PaymentStatus.PartiallyRefunded) && b.CreatedAt >= start && b.CreatedAt <= end)
                .ToListAsync();

            var totalRentalRevenue = completedBookings.Sum(b => b.RentalAmount);
            
            var totalRefunds = await _db.Refunds.AsNoTracking()
                .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
                .SumAsync(r => r.Amount);
                
            var totalSuccessfulPayments = await _db.Payments.AsNoTracking()
                .Where(p => p.PaymentStatus == PaymentStatus.Succeeded && p.CreatedAt >= start && p.CreatedAt <= end)
                .SumAsync(p => p.Amount);

            // Revenue Trend (Last 12 months)
            var trendStart = DateTime.UtcNow.AddMonths(-11).Date;
            trendStart = new DateTime(trendStart.Year, trendStart.Month, 1);
            
            // Only using RentalAmount for trend where Payment is succeeded
            var twelveMonthBookings = await _db.Bookings.AsNoTracking()
                .Where(b => (b.PaymentStatus == PaymentStatus.Succeeded || b.PaymentStatus == PaymentStatus.PartiallyRefunded) && b.CreatedAt >= trendStart)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new 
                { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Revenue = g.Sum(x => x.RentalAmount) 
                })
                .ToListAsync();

            var revenueTrend = new List<RevenueTrendItemDto>();
            for (int i = 0; i < 12; i++)
            {
                var monthDate = trendStart.AddMonths(i);
                var revenue = twelveMonthBookings
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
            var bookingDistribution = await _db.Bookings.AsNoTracking()
                .Where(b => b.CreatedAt >= start && b.CreatedAt <= end)
                .GroupBy(b => b.Status)
                .Select(g => new BookingStatusItemDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            // Top Equipment
            var topEquipment = await _db.Equipment.AsNoTracking()
                .Include(e => e.Business)
                .Include(e => e.Category)
                .Select(e => new
                {
                    Equipment = e,
                    RentalCount = e.Bookings.Count(b => b.Status == BookingStatus.Completed),
                    Revenue = e.Bookings.Where(b => b.PaymentStatus == PaymentStatus.Succeeded || b.PaymentStatus == PaymentStatus.PartiallyRefunded).Sum(b => (decimal?)b.RentalAmount) ?? 0,
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
                AverageRating = x.AvgRating,
                Status = x.Equipment.Status.ToString()
            }).ToList();

            // Top Categories
            var topCategories = await _db.Categories.AsNoTracking()
                .Select(c => new TopCategoryItemDto
                {
                    CategoryName = c.Name,
                    EquipmentCount = c.Equipment.Count,
                    RentalCount = c.Equipment.SelectMany(e => e.Bookings).Count(b => b.Status == BookingStatus.Completed),
                    Revenue = c.Equipment.SelectMany(e => e.Bookings).Where(b => b.PaymentStatus == PaymentStatus.Succeeded || b.PaymentStatus == PaymentStatus.PartiallyRefunded).Sum(b => (decimal?)b.RentalAmount) ?? 0
                })
                .OrderByDescending(x => x.RentalCount)
                .Take(5)
                .ToListAsync();

            // Top Businesses
            var topBusinesses = await _db.Businesses.AsNoTracking()
                .Select(b => new
                {
                    Business = b,
                    CompletedRentals = b.Bookings.Count(bk => bk.Status == BookingStatus.Completed),
                    Revenue = b.Bookings.Where(bk => bk.PaymentStatus == PaymentStatus.Succeeded || bk.PaymentStatus == PaymentStatus.PartiallyRefunded).Sum(bk => (decimal?)bk.RentalAmount) ?? 0,
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
                AvailableEquipment = availableEquipment,
                RentedEquipment = rentedEquipment,
                MaintenanceEquipment = maintenanceEquipment,
                InactiveEquipment = inactiveEquipment,

                TotalBookings = totalBookings,
                PendingBookings = pendingBookings,
                ConfirmedBookings = confirmedBookings,
                CancelledBookings = cancelledBookings,
                CompletedBookings = completedBookingsCount,

                ActiveRentals = activeRentals,
                CompletedRentals = completedRentals,
                OverdueRentals = overdueRentals,
                ReturnedRentals = returnedRentals,

                TotalRentalRevenue = totalRentalRevenue,
                TotalRefunds = totalRefunds,
                TotalSuccessfulPayments = totalSuccessfulPayments,
                
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

            var totalEquipment = await _db.Equipment.CountAsync(e => e.BusinessId == businessId);
            var activeEquipment = await _db.Equipment.CountAsync(e => e.BusinessId == businessId && e.IsActive);
            
            var totalBookings = await _db.Bookings.CountAsync(b => b.BusinessId == businessId && b.CreatedAt >= start && b.CreatedAt <= end);
            var pendingBookings = await _db.Bookings.CountAsync(b => b.BusinessId == businessId && b.Status == BookingStatus.Pending);
            var confirmedBookings = await _db.Bookings.CountAsync(b => b.BusinessId == businessId && b.Status == BookingStatus.Confirmed);
            var cancelledBookings = await _db.Bookings.CountAsync(b => b.BusinessId == businessId && b.Status == BookingStatus.Cancelled);
            var completedBookingsCount = await _db.Bookings.CountAsync(b => b.BusinessId == businessId && b.Status == BookingStatus.Completed);
            
            var upcomingRentals = await _db.Rentals.CountAsync(r => r.BusinessId == businessId && (r.Status == RentalStatus.ReadyForPickup || r.Status == RentalStatus.NotReady));
            var activeRentals = await _db.Rentals.CountAsync(r => r.BusinessId == businessId && (r.Status == RentalStatus.Active));
            var completedRentals = await _db.Rentals.CountAsync(r => r.BusinessId == businessId && r.Status == RentalStatus.Completed);
            var overdueRentals = await _db.Rentals.CountAsync(r => r.BusinessId == businessId && r.Status == RentalStatus.Active && r.ExpectedReturnDate < DateTime.UtcNow);
            
            var completedBookings = await _db.Bookings.AsNoTracking()
                .Where(b => b.BusinessId == businessId && (b.PaymentStatus == PaymentStatus.Succeeded || b.PaymentStatus == PaymentStatus.PartiallyRefunded) && b.CreatedAt >= start && b.CreatedAt <= end)
                .ToListAsync();

            var grossRentalRevenue = completedBookings.Sum(b => b.RentalAmount);

            var totalRefunds = await _db.Refunds.AsNoTracking()
                .Where(r => r.Booking.BusinessId == businessId && r.CreatedAt >= start && r.CreatedAt <= end)
                .SumAsync(r => r.Amount);

            var avgRating = await _db.Reviews.AsNoTracking()
                .Where(r => r.BusinessId == businessId && r.Status == ReviewStatus.Published)
                .AverageAsync(r => (double?)r.BusinessRating) ?? 0;
                
            var totalReviews = await _db.Reviews.AsNoTracking()
                .CountAsync(r => r.BusinessId == businessId && r.Status == ReviewStatus.Published);

            // Revenue Trend (Last 12 months)
            var trendStart = DateTime.UtcNow.AddMonths(-11).Date;
            trendStart = new DateTime(trendStart.Year, trendStart.Month, 1);
            
            var twelveMonthBookings = await _db.Bookings.AsNoTracking()
                .Where(b => b.BusinessId == businessId && (b.PaymentStatus == PaymentStatus.Succeeded || b.PaymentStatus == PaymentStatus.PartiallyRefunded) && b.CreatedAt >= trendStart)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new 
                { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Revenue = g.Sum(x => x.RentalAmount) 
                })
                .ToListAsync();

            var revenueTrend = new List<RevenueTrendItemDto>();
            for (int i = 0; i < 12; i++)
            {
                var monthDate = trendStart.AddMonths(i);
                var revenue = twelveMonthBookings
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
            var bookingDistribution = await _db.Bookings.AsNoTracking()
                .Where(b => b.BusinessId == businessId && b.CreatedAt >= start && b.CreatedAt <= end)
                .GroupBy(b => b.Status)
                .Select(g => new BookingStatusItemDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            // Equipment Performance
            var equipmentPerformance = await _db.Equipment.AsNoTracking()
                .Include(e => e.Category)
                .Where(e => e.BusinessId == businessId)
                .Select(e => new EquipmentPerformanceItemDto
                {
                    EquipmentName = e.Name,
                    CategoryName = e.Category.Name,
                    Bookings = e.Bookings.Count,
                    CompletedRentals = e.Bookings.Count(b => b.Status == BookingStatus.Completed),
                    Revenue = e.Bookings.Where(b => b.PaymentStatus == PaymentStatus.Succeeded || b.PaymentStatus == PaymentStatus.PartiallyRefunded).Sum(b => (decimal?)b.RentalAmount) ?? 0,
                    AverageRating = e.Reviews.Where(r => r.Status == ReviewStatus.Published).Average(r => (double?)r.EquipmentRating) ?? 0,
                    Status = e.Status.ToString()
                })
                .OrderByDescending(x => x.CompletedRentals)
                .Take(10)
                .ToListAsync();
                
            // Category Performance
            var categoryPerformance = await _db.Categories.AsNoTracking()
                .Select(c => new TopCategoryItemDto
                {
                    CategoryName = c.Name,
                    EquipmentCount = c.Equipment.Count(e => e.BusinessId == businessId),
                    RentalCount = c.Equipment.Where(e => e.BusinessId == businessId).SelectMany(e => e.Bookings).Count(b => b.Status == BookingStatus.Completed),
                    Revenue = c.Equipment.Where(e => e.BusinessId == businessId).SelectMany(e => e.Bookings).Where(b => b.PaymentStatus == PaymentStatus.Succeeded || b.PaymentStatus == PaymentStatus.PartiallyRefunded).Sum(b => (decimal?)b.RentalAmount) ?? 0
                })
                .Where(c => c.EquipmentCount > 0)
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            return new BusinessDashboardDto
            {
                TotalEquipment = totalEquipment,
                ActiveEquipment = activeEquipment,
                
                TotalBookings = totalBookings,
                PendingBookings = pendingBookings,
                ConfirmedBookings = confirmedBookings,
                CancelledBookings = cancelledBookings,
                CompletedBookings = completedBookingsCount,
                
                UpcomingRentals = upcomingRentals,
                ActiveRentals = activeRentals,
                CompletedRentals = completedRentals,
                OverdueRentals = overdueRentals,
                
                GrossRentalRevenue = grossRentalRevenue,
                TotalRefunds = totalRefunds,
                
                AverageRating = avgRating,
                TotalReviews = totalReviews,
                StartDate = start,
                EndDate = end,
                RevenueTrend = revenueTrend,
                BookingStatusDistribution = bookingDistribution,
                EquipmentPerformance = equipmentPerformance,
                CategoryPerformance = categoryPerformance
            };
        }

        public async Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(string customerId)
        {
            var totalBookings = await _db.Bookings.CountAsync(b => b.CustomerId == customerId);
            var confirmedBookings = await _db.Bookings.CountAsync(b => b.CustomerId == customerId && b.Status == BookingStatus.Confirmed);
            var completedBookings = await _db.Bookings.CountAsync(b => b.CustomerId == customerId && b.Status == BookingStatus.Completed);
            var cancelledBookings = await _db.Bookings.CountAsync(b => b.CustomerId == customerId && b.Status == BookingStatus.Cancelled);
            
            var activeRentals = await _db.Rentals.CountAsync(r => r.CustomerId == customerId && (r.Status == RentalStatus.Active));
            var completedRentals = await _db.Rentals.CountAsync(r => r.CustomerId == customerId && r.Status == RentalStatus.Completed);
            var pendingReturns = await _db.Rentals.CountAsync(r => r.CustomerId == customerId && r.Status == RentalStatus.Active);
            
            var wishlistItems = await _db.WishlistItems.CountAsync(w => w.CustomerId == customerId);
            
            var reviewsWritten = await _db.Reviews.CountAsync(r => r.CustomerId == customerId && r.Status == ReviewStatus.Published);
            var avgRatingGiven = await _db.Reviews.AsNoTracking()
                .Where(r => r.CustomerId == customerId && r.Status == ReviewStatus.Published)
                .AverageAsync(r => (double?)((r.EquipmentRating + r.BusinessRating) / 2.0)) ?? 0;

            var userBookings = await _db.Bookings.AsNoTracking()
                .Where(b => b.CustomerId == customerId && (b.PaymentStatus == PaymentStatus.Succeeded || b.PaymentStatus == PaymentStatus.PartiallyRefunded))
                .ToListAsync();

            var totalRentalSpending = userBookings.Sum(b => b.RentalAmount);
            var totalSecurityDepositsPaid = userBookings.Sum(b => b.SecurityDepositAmount);
            
            var totalRefunds = await _db.Refunds.AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .SumAsync(r => r.Amount);

            // Monthly Rentals and Spending
            var trendStart = DateTime.UtcNow.AddMonths(-5).Date;
            trendStart = new DateTime(trendStart.Year, trendStart.Month, 1);

            var sixMonthBookings = await _db.Bookings.AsNoTracking()
                .Where(b => b.CustomerId == customerId && (b.PaymentStatus == PaymentStatus.Succeeded || b.PaymentStatus == PaymentStatus.PartiallyRefunded) && b.CreatedAt >= trendStart)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    Spending = g.Sum(x => x.RentalAmount)
                })
                .ToListAsync();

            var monthlyRentals = new List<MonthlyRentalItemDto>();
            for (int i = 0; i < 6; i++)
            {
                var monthDate = trendStart.AddMonths(i);
                var data = sixMonthBookings
                    .Where(r => r.Year == monthDate.Year && r.Month == monthDate.Month)
                    .FirstOrDefault();

                monthlyRentals.Add(new MonthlyRentalItemDto
                {
                    Month = monthDate.ToString("MMM yyyy"),
                    Count = data?.Count ?? 0,
                    Spending = data?.Spending ?? 0
                });
            }

            // Top Categories
            var topCategories = await _db.Rentals.AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .GroupBy(r => r.Equipment.Category.Name)
                .Select(g => new TopCategoryItemDto
                {
                    CategoryName = g.Key,
                    RentalCount = g.Count(),
                    Revenue = g.Sum(r => r.Booking.RentalAmount)
                })
                .OrderByDescending(x => x.RentalCount)
                .Take(5)
                .ToListAsync();

            return new CustomerStatisticsDto
            {
                TotalBookings = totalBookings,
                ConfirmedBookings = confirmedBookings,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings,
                
                ActiveRentals = activeRentals,
                CompletedRentals = completedRentals,
                PendingReturns = pendingReturns,
                
                TotalRentalSpending = totalRentalSpending,
                TotalSecurityDepositsPaid = totalSecurityDepositsPaid,
                TotalRefunds = totalRefunds,
                
                WishlistItems = wishlistItems,
                ReviewsWritten = reviewsWritten,
                AverageRatingGiven = avgRatingGiven,
                
                MonthlyRentals = monthlyRentals,
                TopCategories = topCategories
            };
        }
    }
}

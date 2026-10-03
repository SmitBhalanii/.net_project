using System;
using System.Collections.Generic;

namespace RentKart.Core.DTOs.Reports
{
    public class AdminDashboardDto
    {
        // KPI Cards
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBusinesses { get; set; }
        public int PendingVendors { get; set; }
        public int ApprovedVendors { get; set; }
        public int SuspendedVendors { get; set; }
        public int TotalEquipment { get; set; }
        public int TotalBookings { get; set; }
        public int ActiveRentals { get; set; }
        public decimal TotalRevenue { get; set; }

        // Date Range
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Charts & Tables
        public List<RevenueTrendItemDto> RevenueTrend { get; set; } = new();
        public List<BookingStatusItemDto> BookingStatusDistribution { get; set; } = new();
        public List<TopCategoryItemDto> TopCategories { get; set; } = new();
        public List<TopEquipmentItemDto> TopEquipment { get; set; } = new();
        public List<TopBusinessItemDto> TopBusinesses { get; set; } = new();
    }

    public class BusinessDashboardDto
    {
        // KPI Cards
        public int TotalEquipment { get; set; }
        public int ActiveEquipment { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ActiveRentals { get; set; }
        public int CompletedRentals { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        // Date Range
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Charts & Tables
        public List<RevenueTrendItemDto> RevenueTrend { get; set; } = new();
        public List<BookingStatusItemDto> BookingStatusDistribution { get; set; } = new();
        public List<EquipmentPerformanceItemDto> EquipmentPerformance { get; set; } = new();
    }

    public class CustomerStatisticsDto
    {
        // KPI Cards
        public int TotalBookings { get; set; }
        public int ActiveRentals { get; set; }
        public int CompletedRentals { get; set; }
        public int CancelledBookings { get; set; }
        public int WishlistItems { get; set; }
        public int ReviewsWritten { get; set; }
        public decimal TotalSpent { get; set; }
        public double AverageRatingGiven { get; set; }

        // Charts & Tables
        public List<MonthlyRentalItemDto> MonthlyRentals { get; set; } = new();
        public List<TopCategoryItemDto> TopCategories { get; set; } = new();
    }

    // Shared Items
    public class RevenueTrendItemDto
    {
        public string Period { get; set; } = null!;
        public decimal Revenue { get; set; }
    }

    public class BookingStatusItemDto
    {
        public string Status { get; set; } = null!;
        public int Count { get; set; }
    }

    public class TopCategoryItemDto
    {
        public string CategoryName { get; set; } = null!;
        public int RentalCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopEquipmentItemDto
    {
        public int Rank { get; set; }
        public string EquipmentName { get; set; } = null!;
        public string BusinessName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public int RentalCount { get; set; }
        public decimal Revenue { get; set; }
        public double AverageRating { get; set; }
    }

    public class TopBusinessItemDto
    {
        public int Rank { get; set; }
        public string BusinessName { get; set; } = null!;
        public int CompletedRentals { get; set; }
        public decimal Revenue { get; set; }
        public double AverageRating { get; set; }
        public int ActiveEquipment { get; set; }
        public int Bookings { get; set; }
    }

    public class EquipmentPerformanceItemDto
    {
        public string EquipmentName { get; set; } = null!;
        public int Bookings { get; set; }
        public int CompletedRentals { get; set; }
        public decimal Revenue { get; set; }
        public double AverageRating { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class MonthlyRentalItemDto
    {
        public string Month { get; set; } = null!;
        public int Count { get; set; }
    }
}

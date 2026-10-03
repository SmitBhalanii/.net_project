using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentKart.Web.ViewModels.Equipment;

namespace RentKart.Web.ViewModels.Marketplace;

public class MarketplaceSearchViewModel
{
    // Search & Filters
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string? Location { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinRating { get; set; }
    public int? BusinessId { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? AvailableOnly { get; set; } // If they explicitly want to check availability regardless of dates

    // Sorting
    public string SortBy { get; set; } = "relevance";

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    // Results & SelectLists
    public List<EquipmentCardViewModel> Results { get; set; } = new List<EquipmentCardViewModel>();
    public IEnumerable<SelectListItem>? Categories { get; set; }
}

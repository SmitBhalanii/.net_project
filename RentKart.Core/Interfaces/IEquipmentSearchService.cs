using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;

namespace RentKart.Core.Interfaces;

public interface IEquipmentSearchService
{
    Task<(List<Equipment> Items, int TotalCount)> SearchEquipmentAsync(
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
        int pageSize);
}

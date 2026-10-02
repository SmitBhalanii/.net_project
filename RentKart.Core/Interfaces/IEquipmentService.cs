using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;

namespace RentKart.Core.Interfaces;

public interface IEquipmentService
{
    Task<Equipment> CreateEquipmentAsync(Equipment equipment, List<EquipmentImage> images);
    Task UpdateEquipmentAsync(Equipment equipment, List<EquipmentImage>? newImages = null, List<int>? imageIdsToRemove = null);
    Task<Equipment?> GetEquipmentByIdAsync(int id);
    Task<IEnumerable<Equipment>> GetEquipmentByBusinessAsync(int businessId);
    Task<IEnumerable<Equipment>> SearchActiveEquipmentAsync(string? searchTerm, int? categoryId, string? city);
    Task<bool> DeactivateEquipmentAsync(int id, int businessId);
    Task<bool> ActivateEquipmentAsync(int id, int businessId);
    Task<IEnumerable<Equipment>> GetFeaturedEquipmentAsync(int count);
}

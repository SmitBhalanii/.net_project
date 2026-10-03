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

public class EquipmentService : IEquipmentService
{
    private readonly ApplicationDbContext _context;

    public EquipmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Equipment> CreateEquipmentAsync(Equipment equipment, List<EquipmentImage> images)
    {
        equipment.CreatedAt = DateTime.UtcNow;
        
        foreach (var img in images)
        {
            img.CreatedAt = DateTime.UtcNow;
            equipment.EquipmentImages.Add(img);
        }

        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync();
        return equipment;
    }

    public async Task UpdateEquipmentAsync(Equipment equipment, List<EquipmentImage>? newImages = null, List<int>? imageIdsToRemove = null)
    {
        equipment.UpdatedAt = DateTime.UtcNow;
        
        if (imageIdsToRemove != null && imageIdsToRemove.Any())
        {
            var imagesToRemove = await _context.EquipmentImages
                .Where(img => imageIdsToRemove.Contains(img.Id) && img.EquipmentId == equipment.Id)
                .ToListAsync();
                
            _context.EquipmentImages.RemoveRange(imagesToRemove);
        }

        if (newImages != null && newImages.Any())
        {
            foreach (var img in newImages)
            {
                img.CreatedAt = DateTime.UtcNow;
                equipment.EquipmentImages.Add(img);
            }
        }

        _context.Equipment.Update(equipment);
        await _context.SaveChangesAsync();
    }

    public async Task<Equipment?> GetEquipmentByIdAsync(int id)
    {
        return await _context.Equipment
            .Include(e => e.Business)
            .Include(e => e.Category)
            .Include(e => e.EquipmentImages)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Equipment>> GetEquipmentByBusinessAsync(int businessId)
    {
        return await _context.Equipment
            .Include(e => e.Category)
            .Include(e => e.EquipmentImages)
            .Where(e => e.BusinessId == businessId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Equipment>> SearchActiveEquipmentAsync(string? searchTerm, int? categoryId, string? city)
    {
        var query = _context.Equipment
            .Include(e => e.Business)
            .Include(e => e.Category)
            .Include(e => e.EquipmentImages)
            .Where(e => e.IsActive 
                     && e.Status == EquipmentStatus.Active
                     && e.Category.IsActive
                     && e.Business.ApprovalStatus == BusinessApprovalStatus.Approved);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(term) || 
                                     e.Brand.ToLower().Contains(term) || 
                                     e.Model.ToLower().Contains(term));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(e => e.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(e => e.City.ToLower() == city.ToLower());
        }

        return await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
    }

    public async Task<bool> DeactivateEquipmentAsync(int id, int businessId)
    {
        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == id && e.BusinessId == businessId);
        if (equipment == null) return false;

        equipment.IsActive = false;
        equipment.Status = EquipmentStatus.Inactive;
        equipment.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateEquipmentAsync(int id, int businessId)
    {
        var equipment = await _context.Equipment.FirstOrDefaultAsync(e => e.Id == id && e.BusinessId == businessId);
        if (equipment == null) return false;

        equipment.IsActive = true;
        equipment.Status = EquipmentStatus.Active;
        equipment.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<Equipment>> GetFeaturedEquipmentAsync(int count)
    {
        return await _context.Equipment
            .Include(e => e.Business)
            .Include(e => e.Category)
            .Include(e => e.EquipmentImages)
            .Where(e => e.IsActive 
                     && e.Status == EquipmentStatus.Active
                     && e.Category.IsActive
                     && e.Business.ApprovalStatus == BusinessApprovalStatus.Approved)
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Equipment>> GetRecentEquipmentAsync(int count)
    {
        return await _context.Equipment
            .Include(e => e.Business)
            .Include(e => e.Category)
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}

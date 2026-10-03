using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;
using RentKart.Infrastructure.Data;

namespace RentKart.Infrastructure.Services;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _context;

    public WishlistService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(string customerId, int equipmentId)
    {
        var exists = await IsInWishlistAsync(customerId, equipmentId);
        if (exists) return false;

        var item = new WishlistItem
        {
            CustomerId = customerId,
            EquipmentId = equipmentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.WishlistItems.Add(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(string customerId, int equipmentId)
    {
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.EquipmentId == equipmentId);
            
        if (item == null) return false;

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsInWishlistAsync(string customerId, int equipmentId)
    {
        return await _context.WishlistItems
            .AnyAsync(w => w.CustomerId == customerId && w.EquipmentId == equipmentId);
    }

    public async Task<IEnumerable<WishlistItem>> GetCustomerWishlistAsync(string customerId)
    {
        return await _context.WishlistItems
            .Include(w => w.Equipment)
            .ThenInclude(e => e.Business)
            .Where(w => w.CustomerId == customerId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }
}

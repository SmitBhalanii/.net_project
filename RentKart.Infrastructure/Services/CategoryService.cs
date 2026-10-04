using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;
using RentKart.Infrastructure.Data;

using Microsoft.Extensions.Caching.Memory;

namespace RentKart.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CacheKeyAll = "Categories_All";
    private const string CacheKeyActive = "Categories_Active";

    public CategoryService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeyAll, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }) ?? new List<Category>();
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeyActive, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }) ?? new List<Category>();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        ClearCache();
        return category;
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        category.UpdatedAt = DateTime.UtcNow;
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        ClearCache();
    }

    private void ClearCache()
    {
        _cache.Remove(CacheKeyAll);
        _cache.Remove(CacheKeyActive);
    }

    public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Categories.Where(c => c.Name == name);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }
}

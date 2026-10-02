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

public class BusinessService : IBusinessService
{
    private readonly ApplicationDbContext _context;

    public BusinessService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Business?> GetBusinessByIdAsync(int id)
    {
        return await _context.Businesses.FindAsync(id);
    }

    public async Task<Business?> GetBusinessByUserIdAsync(string userId)
    {
        return await _context.Businesses.FirstOrDefaultAsync(b => b.UserId == userId);
    }

    public async Task<Business?> GetBusinessProfileAsync(int id)
    {
        return await _context.Businesses
            .Include(b => b.Equipment.Where(e => e.IsActive && e.Category.IsActive))
                .ThenInclude(e => e.EquipmentImages)
            .Include(b => b.Equipment.Where(e => e.IsActive && e.Category.IsActive))
                .ThenInclude(e => e.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Business>> GetApprovedBusinessesAsync(string? searchTerm = null, string? city = null)
    {
        var query = _context.Businesses
            .Include(b => b.Equipment.Where(e => e.IsActive && e.Category.IsActive))
            .Where(b => b.ApprovalStatus == BusinessApprovalStatus.Approved);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(b => b.BusinessName.ToLower().Contains(searchTerm) || 
                                     (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(b => b.City.ToLower() == city.ToLower());
        }

        return await query.OrderBy(b => b.BusinessName).ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAvailableCitiesAsync()
    {
        return await _context.Businesses
            .Where(b => b.ApprovalStatus == BusinessApprovalStatus.Approved)
            .Select(b => b.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task UpdateBusinessProfileAsync(Business business)
    {
        var existing = await _context.Businesses.FindAsync(business.Id);
        if (existing == null) throw new Exception("Business not found");

        existing.BusinessName = business.BusinessName;
        existing.Description = business.Description;
        existing.Address = business.Address;
        existing.City = business.City;
        existing.State = business.State;
        existing.PostalCode = business.PostalCode;
        existing.Email = business.Email;
        existing.PhoneNumber = business.PhoneNumber;
        existing.Website = business.Website;
        
        if (business.LogoPath != null)
        {
            existing.LogoPath = business.LogoPath;
        }

        existing.UpdatedAt = DateTime.UtcNow;

        _context.Businesses.Update(existing);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Business>> GetAllBusinessesAsync()
    {
        return await _context.Businesses
            .Include(b => b.User)
            .Include(b => b.Equipment)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateBusinessStatusAsync(int id, BusinessApprovalStatus status)
    {
        var business = await _context.Businesses.FindAsync(id);
        if (business != null)
        {
            business.ApprovalStatus = status;
            business.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

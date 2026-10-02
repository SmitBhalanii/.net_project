using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;
using RentKart.Core.Enums;

namespace RentKart.Core.Interfaces;

public interface IBusinessService
{
    Task<Business?> GetBusinessByIdAsync(int id);
    Task<Business?> GetBusinessByUserIdAsync(string userId);
    Task<Business?> GetBusinessProfileAsync(int id);
    Task<IEnumerable<Business>> GetApprovedBusinessesAsync(string? searchTerm = null, string? city = null);
    Task<IEnumerable<string>> GetAvailableCitiesAsync();
    Task UpdateBusinessProfileAsync(Business business);
    
    // Admin functions
    Task<IEnumerable<Business>> GetAllBusinessesAsync();
    Task UpdateBusinessStatusAsync(int id, BusinessApprovalStatus status);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;

namespace RentKart.Core.Interfaces;

public interface IWishlistService
{
    Task<bool> AddAsync(string customerId, int equipmentId);
    Task<bool> RemoveAsync(string customerId, int equipmentId);
    Task<bool> IsInWishlistAsync(string customerId, int equipmentId);
    Task<IEnumerable<WishlistItem>> GetCustomerWishlistAsync(string customerId);
}

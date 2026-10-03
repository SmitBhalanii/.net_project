using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;

namespace RentKart.Core.Interfaces;

public interface IUserManagementService
{
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task UpdateUserStatusAsync(string userId, bool isActive);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;

namespace RentKart.Core.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null);
}

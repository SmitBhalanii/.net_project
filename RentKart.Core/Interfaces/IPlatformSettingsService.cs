using System.Threading.Tasks;

namespace RentKart.Core.Interfaces;

public interface IPlatformSettingsService
{
    Task<string?> GetSettingValueAsync(string key);
    Task SetSettingValueAsync(string key, string value, string? description = null);
    Task<bool> IsMarketplaceActiveAsync();
}

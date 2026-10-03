using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Core.Interfaces;
using RentKart.Infrastructure.Data;

namespace RentKart.Infrastructure.Services;

public class PlatformSettingsService : IPlatformSettingsService
{
    private readonly ApplicationDbContext _db;

    public PlatformSettingsService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string?> GetSettingValueAsync(string key)
    {
        var setting = await _db.PlatformSettings.FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task SetSettingValueAsync(string key, string value, string? description = null)
    {
        var setting = await _db.PlatformSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null)
        {
            setting = new PlatformSetting
            {
                Key = key,
                Value = value,
                Description = description,
                UpdatedAt = DateTime.UtcNow
            };
            _db.PlatformSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            if (description != null)
            {
                setting.Description = description;
            }
            setting.UpdatedAt = DateTime.UtcNow;
            _db.PlatformSettings.Update(setting);
        }
        await _db.SaveChangesAsync();
    }

    public async Task<bool> IsMarketplaceActiveAsync()
    {
        var val = await GetSettingValueAsync("MarketplaceActive");
        return val != "False"; // Default to true
    }
}

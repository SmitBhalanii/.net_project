using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentKart.Core.Constants;
using RentKart.Core.Entities;
using System;
using System.Threading.Tasks;

namespace RentKart.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        string[] roleNames = { RoleNames.Admin, RoleNames.Business, RoleNames.Customer };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    logger.LogError("Error creating role {Role}", roleName);
                }
            }
        }

        string adminEmail = "admin@rentkart.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                DisplayName = "System Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, RoleNames.Admin);
                logger.LogInformation("Default admin user created successfully.");
            }
            else
            {
                logger.LogError("Failed to create default admin user.");
                foreach (var error in result.Errors)
                {
                    logger.LogError("Error: {Code} - {Description}", error.Code, error.Description);
                }
            }
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentKart.Core.Constants;
using RentKart.Core.Entities;
using System;
using System.Linq;
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

        string[] roleNames = { RoleNames.Admin, RoleNames.Business, RoleNames.Customer, RoleNames.VendorStaff };

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
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await dbContext.Database.MigrateAsync();
        
        if (!dbContext.Categories.Any())
        {
            var categories = new Category[]
            {
                new Category { Name = "Photography", Description = "Cameras, lenses, and accessories.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Videography", Description = "Video cameras, gimbals, and more.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Drones", Description = "Aerial photography and videography.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Audio Equipment", Description = "Microphones, recorders, and mixers.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Lighting Equipment", Description = "Studio lights, strobes, and modifiers.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Event Equipment", Description = "Tents, tables, chairs, and decor.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Gaming Consoles", Description = "PlayStation, Xbox, and Nintendo.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Laptops", Description = "MacBooks, Windows, and gaming laptops.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Projectors", Description = "Home theater and business projectors.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Camping Equipment", Description = "Tents, sleeping bags, and gear.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Power Tools", Description = "Drills, saws, and construction tools.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "VR Devices", Description = "Meta Quest, HTC Vive, and accessories.", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            await dbContext.Categories.AddRangeAsync(categories);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seed categories created successfully.");
        }

        string businessEmail = "business@rentkart.com";
        var businessUser = await userManager.FindByEmailAsync(businessEmail);
        
        if (businessUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = businessEmail,
                Email = businessEmail,
                FirstName = "Demo",
                LastName = "Business",
                DisplayName = "Demo Studio",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Business@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, RoleNames.Business);
                
                var business = new Business
                {
                    UserId = user.Id,
                    OwnerName = "Demo Business",
                    BusinessName = "Demo Studio",
                    Email = businessEmail,
                    PhoneNumber = "1234567890",
                    Address = "123 Demo St",
                    City = "Vadodara",
                    State = "Gujarat",
                    PostalCode = "390001",
                    Description = "A demo studio for testing.",
                    ApprovalStatus = RentKart.Core.Enums.BusinessApprovalStatus.Approved,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await dbContext.Businesses.AddAsync(business);
                await dbContext.SaveChangesAsync();

                // Seed some equipment for this business
                var photoCategory = dbContext.Categories.FirstOrDefault(c => c.Name == "Photography");
                if (photoCategory != null)
                {
                    var eq1 = new Equipment
                    {
                        BusinessId = business.Id,
                        CategoryId = photoCategory.Id,
                        Name = "Sony A7 IV Camera",
                        Brand = "Sony",
                        Model = "A7 IV",
                        Description = "Mirrorless camera with 33MP.",
                        RentalPrice = 1500,
                        RentalPeriod = RentKart.Core.Enums.RentalPeriod.Daily,
                        SecurityDeposit = 10000,
                        Quantity = 1,
                        Condition = RentKart.Core.Enums.EquipmentCondition.Excellent,
                        Status = RentKart.Core.Enums.EquipmentStatus.Active,
                        IsActive = true,
                        City = "Vadodara",
                        State = "Gujarat",
                        PostalCode = "390001",
                        CreatedAt = DateTime.UtcNow
                    };
                    await dbContext.Equipment.AddAsync(eq1);
                    await dbContext.SaveChangesAsync();
                }

                logger.LogInformation("Default business and equipment created successfully.");
            }
        }
    }
}

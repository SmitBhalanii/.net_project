using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Core.Enums;
using RentKart.Infrastructure.Data;
using RentKart.Infrastructure.Services;
using Xunit;

namespace RentKart.Tests.Services;

public class BusinessServiceTests
{
    private DbContextOptions<ApplicationDbContext> GetDbOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task GetApprovedBusinessesAsync_ShouldReturnOnlyApproved()
    {
        // Arrange
        var options = GetDbOptions("ApprovedDb");
        using var context = new ApplicationDbContext(options);
        
        context.Businesses.AddRange(
            new Business { Id = 1, BusinessName = "Approved 1", UserId = "1", OwnerName = "O1", Email = "e@e.com", PhoneNumber = "1", Address = "A1", City = "CityA", State = "StateA", PostalCode = "0", ApprovalStatus = BusinessApprovalStatus.Approved },
            new Business { Id = 2, BusinessName = "Pending 1", UserId = "2", OwnerName = "O2", Email = "e2@e.com", PhoneNumber = "2", Address = "A2", City = "CityA", State = "StateA", PostalCode = "0", ApprovalStatus = BusinessApprovalStatus.Pending },
            new Business { Id = 3, BusinessName = "Rejected 1", UserId = "3", OwnerName = "O3", Email = "e3@e.com", PhoneNumber = "3", Address = "A3", City = "CityA", State = "StateA", PostalCode = "0", ApprovalStatus = BusinessApprovalStatus.Rejected },
            new Business { Id = 4, BusinessName = "Suspended 1", UserId = "4", OwnerName = "O4", Email = "e4@e.com", PhoneNumber = "4", Address = "A4", City = "CityA", State = "StateA", PostalCode = "0", ApprovalStatus = BusinessApprovalStatus.Suspended }
        );
        await context.SaveChangesAsync();

        var service = new BusinessService(context);

        // Act
        var result = await service.GetApprovedBusinessesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Approved 1", result.First().BusinessName);
    }
}

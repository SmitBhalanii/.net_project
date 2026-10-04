using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentKart.Core.Entities;
using RentKart.Core.Enums;
using RentKart.Infrastructure.Data;
using RentKart.Infrastructure.Services;
using Xunit;

namespace RentKart.Tests.Services
{
    public class ReportingServiceTests
    {
        private DbContextOptions<ApplicationDbContext> GetDbOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        private ApplicationDbContext GetContext(string dbName)
        {
            var options = GetDbOptions(dbName);
            var context = new ApplicationDbContext(options);
            return context;
        }

        [Fact]
        public async Task GetAdminDashboardAsync_ReturnsCorrectTotals()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetContext(dbName))
            {
                var customer1 = new ApplicationUser { Id = "c1", UserName = "c1@test.com", FirstName = "C1", LastName = "Last", DisplayName = "C1 Last" };
                var businessUser = new ApplicationUser { Id = "b1", UserName = "b1@test.com", FirstName = "B1", LastName = "Last", DisplayName = "B1 Last" };
                context.Users.AddRange(customer1, businessUser);
                
                var business = new Business { Id = 1, UserId = "b1", BusinessName = "Test Business", IsActive = true, ApprovalStatus = BusinessApprovalStatus.Approved, Address = "A", City = "C", Email = "E@e.com", OwnerName = "O", PhoneNumber = "123", PostalCode = "123", State = "S" };
                context.Businesses.Add(business);
                
                var category = new Category { Id = 1, Name = "Cameras" };
                context.Categories.Add(category);
                
                var equipment = new Equipment { Id = 1, BusinessId = 1, CategoryId = 1, Name = "Camera 1", IsActive = true, Brand = "B", City = "C", Model = "M", PostalCode = "P", State = "S", Description = "D" };
                context.Equipment.Add(equipment);
                
                var booking = new Booking 
                { 
                    Id = 1, BusinessId = 1, EquipmentId = 1, CustomerId = "c1", 
                    Status = BookingStatus.Completed, PaymentStatus = PaymentStatus.Succeeded, RentalAmount = 1000, CreatedAt = DateTime.UtcNow 
                };
                context.Bookings.Add(booking);
                
                var payment = new Payment 
                { 
                    Id = 1, BookingId = 1, CustomerId = "c1", 
                    Amount = 1000, PaymentStatus = PaymentStatus.Succeeded, CreatedAt = DateTime.UtcNow 
                };
                context.Payments.Add(payment);

                await context.SaveChangesAsync();
            }

            using (var context = GetContext(dbName))
            {
                var service = new ReportingService(context);
                var result = await service.GetAdminDashboardAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

                Assert.Equal(2, result.TotalUsers);
                Assert.Equal(1, result.TotalBusinesses);
                Assert.Equal(1, result.TotalCustomers);
                Assert.Equal(1, result.TotalEquipment);
                Assert.Equal(1, result.TotalBookings);
                Assert.Equal(1000, result.TotalRentalRevenue);
                Assert.Single(result.BookingStatusDistribution);
                Assert.Equal("Completed", result.BookingStatusDistribution[0].Status);
            }
        }

        [Fact]
        public async Task GetBusinessDashboardAsync_ReturnsIsolatedData()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetContext(dbName))
            {
                var customer1 = new ApplicationUser { Id = "c1", UserName = "c1@test.com", FirstName = "C1", LastName = "Last", DisplayName = "C1 Last" };
                context.Users.Add(customer1);
                
                var b1 = new Business { Id = 1, UserId = "b1", BusinessName = "B1", Address = "A", City = "C", Email = "E@e.com", OwnerName = "O", PhoneNumber = "123", PostalCode = "123", State = "S" };
                var b2 = new Business { Id = 2, UserId = "b2", BusinessName = "B2", Address = "A", City = "C", Email = "E@e.com", OwnerName = "O", PhoneNumber = "123", PostalCode = "123", State = "S" };
                context.Businesses.AddRange(b1, b2);
                
                var eq1 = new Equipment { Id = 1, BusinessId = 1, Name = "E1", Brand = "B", City = "C", Model = "M", PostalCode = "P", State = "S", Description = "D", CategoryId = 1 };
                var eq2 = new Equipment { Id = 2, BusinessId = 2, Name = "E2", Brand = "B", City = "C", Model = "M", PostalCode = "P", State = "S", Description = "D", CategoryId = 1 };
                context.Equipment.AddRange(eq1, eq2);
                
                var booking1 = new Booking { Id = 1, BusinessId = 1, EquipmentId = 1, CustomerId = "c1", Status = BookingStatus.Completed, PaymentStatus = PaymentStatus.Succeeded, RentalAmount = 1000, CreatedAt = DateTime.UtcNow };
                var booking2 = new Booking { Id = 2, BusinessId = 2, EquipmentId = 2, CustomerId = "c1", Status = BookingStatus.Completed, PaymentStatus = PaymentStatus.Succeeded, RentalAmount = 2000, CreatedAt = DateTime.UtcNow };
                context.Bookings.AddRange(booking1, booking2);

                var payment1 = new Payment { Id = 1, BookingId = 1, CustomerId = "c1", Amount = 1000, PaymentStatus = PaymentStatus.Succeeded, CreatedAt = DateTime.UtcNow };
                var payment2 = new Payment { Id = 2, BookingId = 2, CustomerId = "c1", Amount = 2000, PaymentStatus = PaymentStatus.Succeeded, CreatedAt = DateTime.UtcNow };
                context.Payments.AddRange(payment1, payment2);

                await context.SaveChangesAsync();
            }

            using (var context = GetContext(dbName))
            {
                var service = new ReportingService(context);
                var resultB1 = await service.GetBusinessDashboardAsync(1, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

                Assert.Equal(1, resultB1.TotalEquipment);
                Assert.Equal(1, resultB1.TotalBookings);
                Assert.Equal(1000, resultB1.GrossRentalRevenue);
                
                var resultB2 = await service.GetBusinessDashboardAsync(2, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

                Assert.Equal(1, resultB2.TotalEquipment);
                Assert.Equal(1, resultB2.TotalBookings);
                Assert.Equal(2000, resultB2.GrossRentalRevenue);
            }
        }
        
        [Fact]
        public async Task GetCustomerStatisticsAsync_ReturnsIsolatedData()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetContext(dbName))
            {
                var customer1 = new ApplicationUser { Id = "c1", UserName = "c1@test.com", FirstName = "C1", LastName = "Last", DisplayName = "C1 Last" };
                var customer2 = new ApplicationUser { Id = "c2", UserName = "c2@test.com", FirstName = "C2", LastName = "Last", DisplayName = "C2 Last" };
                context.Users.AddRange(customer1, customer2);
                
                var b1 = new Business { Id = 1, UserId = "b1", BusinessName = "B1", Address = "A", City = "C", Email = "E@e.com", OwnerName = "O", PhoneNumber = "123", PostalCode = "123", State = "S" };
                context.Businesses.Add(b1);
                
                var eq1 = new Equipment { Id = 1, BusinessId = 1, Name = "E1", Brand = "B", City = "C", Model = "M", PostalCode = "P", State = "S", Description = "D", CategoryId = 1 };
                context.Equipment.Add(eq1);
                
                var booking1 = new Booking { Id = 1, BusinessId = 1, EquipmentId = 1, CustomerId = "c1", Status = BookingStatus.Completed, PaymentStatus = PaymentStatus.Succeeded, RentalAmount = 1000, CreatedAt = DateTime.UtcNow };
                var booking2 = new Booking { Id = 2, BusinessId = 1, EquipmentId = 1, CustomerId = "c2", Status = BookingStatus.Completed, PaymentStatus = PaymentStatus.Succeeded, RentalAmount = 2000, CreatedAt = DateTime.UtcNow };
                context.Bookings.AddRange(booking1, booking2);

                var payment1 = new Payment { Id = 1, BookingId = 1, CustomerId = "c1", Amount = 1000, PaymentStatus = PaymentStatus.Succeeded, CreatedAt = DateTime.UtcNow };
                var payment2 = new Payment { Id = 2, BookingId = 2, CustomerId = "c2", Amount = 2000, PaymentStatus = PaymentStatus.Succeeded, CreatedAt = DateTime.UtcNow };
                context.Payments.AddRange(payment1, payment2);

                await context.SaveChangesAsync();
            }

            using (var context = GetContext(dbName))
            {
                var service = new ReportingService(context);
                
                var resultC1 = await service.GetCustomerStatisticsAsync("c1");
                Assert.Equal(1, resultC1.TotalBookings);
                Assert.Equal(1000, resultC1.TotalRentalSpending);

                var resultC2 = await service.GetCustomerStatisticsAsync("c2");
                Assert.Equal(1, resultC2.TotalBookings);
                Assert.Equal(2000, resultC2.TotalRentalSpending);
            }
        }

        [Fact]
        public async Task Admin_Revenue_Does_Not_Include_Failed_Or_Pending_Payments()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetContext(dbName))
            {
                var b1 = new Booking { Id = 1, BusinessId = 1, EquipmentId = 1, CustomerId = "c1", Status = BookingStatus.Completed, PaymentStatus = PaymentStatus.Succeeded, RentalAmount = 1000, CreatedAt = DateTime.UtcNow };
                var b2 = new Booking { Id = 2, BusinessId = 1, EquipmentId = 1, CustomerId = "c1", Status = BookingStatus.Completed, PaymentStatus = PaymentStatus.Failed, RentalAmount = 2000, CreatedAt = DateTime.UtcNow };
                var b3 = new Booking { Id = 3, BusinessId = 1, EquipmentId = 1, CustomerId = "c1", Status = BookingStatus.Completed, PaymentStatus = PaymentStatus.Pending, RentalAmount = 3000, CreatedAt = DateTime.UtcNow };
                context.Bookings.AddRange(b1, b2, b3);

                await context.SaveChangesAsync();
            }

            using (var context = GetContext(dbName))
            {
                var service = new ReportingService(context);
                var result = await service.GetAdminDashboardAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

                Assert.Equal(1000, result.TotalRentalRevenue);
            }
        }

        [Fact]
        public async Task DateFilter_ReturnsEmpty_WhenNoDataInDateRange()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetContext(dbName))
            {
                var booking = new Booking { Id = 1, BusinessId = 1, EquipmentId = 1, CustomerId = "c1", Status = BookingStatus.Completed, CreatedAt = DateTime.UtcNow.AddDays(-10) };
                context.Bookings.Add(booking);
                
                var payment = new Payment { Id = 1, BookingId = 1, CustomerId = "c1", Amount = 1000, PaymentStatus = PaymentStatus.Succeeded, CreatedAt = DateTime.UtcNow.AddDays(-10) };
                context.Payments.Add(payment);

                await context.SaveChangesAsync();
            }

            using (var context = GetContext(dbName))
            {
                var service = new ReportingService(context);
                var result = await service.GetAdminDashboardAsync(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow);

                Assert.Equal(0, result.TotalBookings);
                Assert.Equal(0, result.TotalRentalRevenue);
            }
        }
    }
}






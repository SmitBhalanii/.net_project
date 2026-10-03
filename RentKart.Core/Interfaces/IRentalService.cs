using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;
using RentKart.Core.Enums;

namespace RentKart.Core.Interfaces;

public interface IRentalService
{
    Task<Rental?> GetRentalByIdAsync(int rentalId);
    Task<Rental?> GetRentalByBookingIdAsync(int bookingId);
    Task<Rental?> GetRentalByBookingNumberAsync(string bookingNumber);
    Task<IEnumerable<Rental>> GetBusinessRentalsAsync(int businessId);
    Task<IEnumerable<Rental>> GetCustomerRentalsAsync(string customerId);
    Task<Rental> PrepareForPickupAsync(int bookingId);
    Task<Rental> IssueEquipmentAsync(int rentalId, string staffId, EquipmentCondition condition, string? notes);
    Task<Rental> ReturnEquipmentAsync(int rentalId, string staffId, EquipmentCondition condition, bool damageFound, string? damageDescription, string? notes);
    Task<Rental> CompleteRentalAsync(int rentalId, string staffId);
    Task<Equipment?> GetEquipmentByCodeAsync(string equipmentCode);
    Task ProcessDueNotificationsAsync();
}

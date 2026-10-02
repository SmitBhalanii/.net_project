using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;

namespace RentKart.Core.Interfaces;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Booking booking);
    Task<Booking?> GetBookingByIdAsync(int id);
    Task<Booking?> GetBookingByNumberAsync(string bookingNumber);
    Task<IEnumerable<Booking>> GetCustomerBookingsAsync(string customerId);
    Task<IEnumerable<Booking>> GetBusinessBookingsAsync(int businessId);
    Task<bool> IsEquipmentAvailableAsync(int equipmentId, DateTime startDate, DateTime endDate);
    Task<bool> ApproveBookingAsync(int id, int businessId, string? businessNote);
    Task<bool> RejectBookingAsync(int id, int businessId, string? businessNote);
    Task<bool> CancelBookingAsync(int id, string customerId, string? customerNote);
    string GenerateBookingNumber();
}

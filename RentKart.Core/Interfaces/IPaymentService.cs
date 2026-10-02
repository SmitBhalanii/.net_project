using System.Collections.Generic;
using System.Threading.Tasks;
using RentKart.Core.Entities;
using RentKart.Core.Enums;

namespace RentKart.Core.Interfaces;

public interface IPaymentService
{
    Task<Payment?> GetPaymentByIdAsync(int id);
    Task<Payment?> GetPaymentByReferenceAsync(string paymentReference);
    Task<IEnumerable<Payment>> GetCustomerPaymentsAsync(string customerId);
    Task<IEnumerable<Payment>> GetBusinessPaymentsAsync(int businessId);
    Task<Payment> ProcessDemoPaymentAsync(int bookingId, string customerId, PaymentMethod method);
}

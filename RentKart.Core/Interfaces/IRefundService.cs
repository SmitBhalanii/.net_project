using System.Threading.Tasks;
using System.Collections.Generic;
using RentKart.Core.Entities;

namespace RentKart.Core.Interfaces;

public interface IRefundService
{
    Task<Refund> ProcessRefundAsync(int paymentId, string customerId, decimal amount, string reason, string staffId);
    Task<IEnumerable<Refund>> GetCustomerRefundsAsync(string customerId);
    Task<IEnumerable<Refund>> GetBusinessRefundsAsync(int businessId);
}

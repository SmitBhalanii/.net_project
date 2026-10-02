using System.Threading.Tasks;
using RentKart.Core.Enums;

namespace RentKart.Core.Interfaces;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public PaymentMethod Method { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
}

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public PaymentStatus Status { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
}

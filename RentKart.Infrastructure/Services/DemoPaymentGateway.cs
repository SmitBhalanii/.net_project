using System;
using System.Threading.Tasks;
using RentKart.Core.Enums;
using RentKart.Core.Interfaces;

namespace RentKart.Infrastructure.Services;

public class DemoPaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        // Simulate network delay
        // await Task.Delay(1000); 

        var transactionRef = $"DEMO-TXN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        if (request.Method == PaymentMethod.MockCard || request.Method == PaymentMethod.MockUPI)
        {
            return Task.FromResult(new PaymentResult
            {
                IsSuccess = true,
                Status = PaymentStatus.Succeeded,
                TransactionReference = transactionRef
            });
        }

        // Simulate failure if anything else or intentional failure scenario
        return Task.FromResult(new PaymentResult
        {
            IsSuccess = false,
            Status = PaymentStatus.Failed,
            TransactionReference = transactionRef,
            ErrorMessage = "Demo payment failed intentionally."
        });
    }
}

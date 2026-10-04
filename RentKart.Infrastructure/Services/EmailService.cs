using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RentKart.Core.Interfaces;

namespace RentKart.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        // For MVP, we just log the email content
        _logger.LogInformation("--- MOCK EMAIL DELIVERED ---");
        _logger.LogInformation($"To: {to}");
        _logger.LogInformation($"Subject: {subject}");
        _logger.LogInformation($"IsHtml: {isHtml}");
        _logger.LogInformation($"Body: {body}");
        _logger.LogInformation("----------------------------");
        
        return Task.CompletedTask;
    }

    public Task SendTemplateEmailAsync(string to, string templateName, object model)
    {
        // Example templates
        string subject = templateName switch
        {
            "BookingConfirmation" => "Your Booking is Confirmed!",
            "PaymentReceipt" => "Your Payment Receipt",
            "InvoiceGenerated" => "Your Invoice is Ready",
            "VendorApproved" => "Welcome to RentKart - Vendor Account Approved",
            _ => $"Notification: {templateName}"
        };

        // For MVP, we serialize the model to json as the body to simulate template rendering
        string body = $"Template: {templateName}\nData: {System.Text.Json.JsonSerializer.Serialize(model)}";

        return SendEmailAsync(to, subject, body, true);
    }
}

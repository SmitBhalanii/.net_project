using System.Threading.Tasks;

namespace RentKart.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendTemplateEmailAsync(string to, string templateName, object model);
}

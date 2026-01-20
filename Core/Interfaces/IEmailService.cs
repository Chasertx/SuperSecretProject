namespace PortfolioPro.interfaces; // Match the Service's using statement

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
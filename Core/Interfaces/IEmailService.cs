namespace PortfolioPro.interfaces;

public interface IEmailService
{
    // Rule: Send a message to a specific email address with a title and the main text included
    Task SendEmailAsync(string to, string subject, string body);
}
namespace PortfolioPro.interfaces;

public interface IEmailService
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    /// <param name="to">Recipient.</param>
    /// <param name="subject">Subject.</param>
    /// <param name="body">Message in the email.</param>
    /// <returns></returns>
    Task SendEmailAsync(string to, string subject, string body);
}
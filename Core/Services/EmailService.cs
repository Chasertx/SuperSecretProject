using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using PortfolioPro.interfaces; // Unified namespace

namespace PortfolioPro.Core.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();

        var fromName = _config["EmailSettings:FromName"] ?? "Portfolio Pro";
        var fromAddress = _config["EmailSettings:FromEmail"] ?? "noreply@portfoliopro.com";

        email.From.Add(new MailboxAddress(fromName, fromAddress));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = body
        };

        using var smtp = new SmtpClient();

        try
        {
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"] ?? "587");

            // Use SecureSocketOptions.Auto to handle both SSL and STARTTLS
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _config["EmailSettings:Email"],
                _config["EmailSettings:Password"]
            );

            await smtp.SendAsync(email);
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }
}
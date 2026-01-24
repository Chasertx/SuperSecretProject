using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using PortfolioPro.interfaces;
/* If you like sending emails with auto 
generated codes boy howdy do I have 
the thing for you. */
namespace PortfolioPro.Core.Services;
/// <summary>
/// Handles dispatch of emails using Mailkit library and SMTP provider.
/// </summary>
public class EmailService : IEmailService
{
    // Access appsettings.json for config values
    // and sensitive info like SMTP passwords.
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the
    /// email service with the configuration settings.
    /// </summary>
    /// <param name="config"></param>
    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Constructs and sends an HTML-formatted email.
    /// </summary>
    /// <param name="to">The recipient of the email.</param>
    /// <param name="subject">Subject line of the email.</param>
    /// <param name="body">The html content for the content of the email.</param>
    /// <returns></returns>
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Initialize a new Mimemessage object to hold email data.
        var email = new MimeMessage();

        // Retrieves the sender's display name or defaults to "Portfolio Pro".
        var fromName = _config["EmailSettings:FromName"] ?? "Portfolio Pro";
        // Retrieves the sender's email address or defaults to a generic fallback.
        var fromAddress = _config["EmailSettings:FromEmail"] ?? "noreply@portfoliopro.com";

        // Adds the 'from' address to the email.
        email.From.Add(new MailboxAddress(fromName, fromAddress));
        // Parses the recipient string and adds it to the 'To' header.
        email.To.Add(MailboxAddress.Parse(to));
        // Assigns the subject line to the email.
        email.Subject = subject;

        // Creates the HTML body of the email using MimeKit's textpart.
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = body // Sets the actual HTML content.
        };

        // Instantiates the SMTP client to handle the network connection.
        using var smtp = new SmtpClient();

        try
        {
            // Retrieves SMTP server host address (e.g., smtp.gmail.com)
            var host = _config["EmailSettings:Host"];
            // Retrieves the port number and converts it to an integer (defaults to 587). 
            var port = int.Parse(_config["EmailSettings:Port"] ?? "587");
            // Connects to the server using STARTTLS for an encrypted session.
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);

            // Logs into the SMTP server using the provided username and password.
            await smtp.AuthenticateAsync(
                _config["EmailSettings:Email"],
                _config["EmailSettings:Password"]
            );

            // Transmits the email across the network. 
            await smtp.SendAsync(email);
        }
        finally
        {
            // Ensures the connection is closed even if the send fails.
            await smtp.DisconnectAsync(true);
        }
    }
}
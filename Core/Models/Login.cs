namespace PortfolioPro.Core.Models;

public class LoginRequest
{
    // The email address the person typed in to identify which account they want to access
    public string Email { get; set; } = string.Empty;

    // The secret password the person typed in, which the system will check for a match
    public string Password { get; set; } = string.Empty;
}
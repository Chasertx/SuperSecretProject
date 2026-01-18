namespace PortfolioPro.Models;

// Data structure for the login payload
public class LoginRequest
{
    // We use Username as the label, but it will hold the User's Email
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
namespace PortfolioPro.Core.Models;
/**Model for login requests. **/
public class LoginRequest
{
    // The email or username used to identify the user.
    public string Username { get; set; } = string.Empty;

    // The raw password string to be checked.
    public string Password { get; set; } = string.Empty;
}
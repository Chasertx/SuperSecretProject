using System.Text.Json.Serialization;
/** Model for user profiles **/
namespace PortfolioPro.Models;

public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;


    public string Password { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
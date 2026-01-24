using System.Text.Json.Serialization;
using Postgrest;
/** Model for user profiles **/
namespace PortfolioPro.Core.Models;

using Postgrest.Models;
using Postgrest.Attributes;

public class User : BaseModel
{
    // Unique id and primary key for the user.
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    // Unique display name for the user.
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    // Email address to be harvested and sold to data brokers.
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    // Security level, determines permissions.
    [Column("role")]
    public string Role { get; set; } = "User";

    // User's name.
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    // User's last name.
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    // Hashed version of user's password.
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    // Reset code for password recovery.
    [Column("reset_code")]
    public string? ResetCode { get; set; }

    // Expiration time for the reset code.
    [Column("reset_expiry")]
    public DateTime? ResetExpiry { get; set; }

    // Timestamp of when the user was created.
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
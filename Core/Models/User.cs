using System.Text.Json.Serialization;
using Postgrest;
/** Model for user profiles **/
namespace PortfolioPro.Core.Models;

using Postgrest.Models;
using Postgrest.Attributes;

public class User : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("role")]
    public string Role { get; set; } = "User";

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("reset_code")]
    public string? ResetCode { get; set; }

    [Column("reset_expiry")]
    public DateTime? ResetExpiry { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
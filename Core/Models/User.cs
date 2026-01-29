using Postgrest.Models;
using Postgrest.Attributes;

namespace PortfolioPro.Core.Models;

[Table("users")]
public class User : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("role")]
    public string Role { get; set; } = "User";

    // Matches the 'password' column in DB, property named PasswordHash for C# logic
    [Column("password")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("first_name")]
    public string? FirstName { get; set; }

    [Column("last_name")]
    public string? LastName { get; set; }

    // --- Professional Profile ---
    [Column("Title")]
    public string? Title { get; set; }

    [Column("Bio")]
    public string? Bio { get; set; }

    [Column("YearsOfExperience")]
    public int YearsOfExperience { get; set; }

    [Column("ProfileImageUrl")]
    public string? ProfileImageUrl { get; set; }

    [Column("ResumeUrl")]
    public string? ResumeUrl { get; set; }

    // --- Branding & Skills ---
    [Column("Tagline1")]
    public string? Tagline1 { get; set; }

    [Column("Tagline2")]
    public string? Tagline2 { get; set; }

    // FIX: Changed to List<string> to handle JSON arrays correctly
    [Column("FrontendSkills")]
    public string[]? FrontendSkills { get; set; } = Array.Empty<string>();

    [Column("BackendSkills")]
    public string[]? BackendSkills { get; set; } = Array.Empty<string>();

    [Column("DatabaseSkills")]
    public string[]? DatabaseSkills { get; set; } = Array.Empty<string>();

    // --- Socials ---
    [Column("instagram_link")]
    public string? InstagramLink { get; set; }

    [Column("GitHubLink")]
    public string? GitHubLink { get; set; }

    [Column("linkedin_link")]
    public string? LinkedInLink { get; set; }

    // --- Auth Infrastructure ---
    [Column("reset_code")]
    public string? ResetCode { get; set; }

    [Column("reset_expiry")]
    public DateTime? ResetExpiry { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
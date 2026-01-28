using Postgrest.Models;
using Postgrest.Attributes;

namespace PortfolioPro.Core.Models;

[Table("Users")]
public class User : BaseModel
{
    [PrimaryKey("Id", false)]
    public Guid Id { get; set; }

    [Column("Username")]
    public string Username { get; set; } = string.Empty;

    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    [Column("Role")]
    public string Role { get; set; } = string.Empty;

    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("Password")]
    public string Password { get; set; } = string.Empty;

    [Column("FirstName")]
    public string? FirstName { get; set; }

    [Column("LastName")]
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

    [Column("FrontendSkills")]
    public string? FrontendSkills { get; set; }

    [Column("BackendSkills")]
    public string? BackendSkills { get; set; }

    [Column("DatabaseSkills")]
    public string? DatabaseSkills { get; set; }

    // --- Socials ---
    [Column("InstagramLink")]
    public string? InstagramLink { get; set; }

    [Column("GitHubLink")]
    public string? GitHubLink { get; set; }

    [Column("LinkedInLink")]
    public string? LinkedInLink { get; set; }

    // --- Auth Infrastructure (Required by AuthRepo) ---
    [Column("ResetCode")]
    public string? ResetCode { get; set; }

    [Column("ResetExpiry")]
    public DateTime? ResetExpiry { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
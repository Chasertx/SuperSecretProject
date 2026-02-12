using Postgrest.Models;
using Postgrest.Attributes;

namespace PortfolioPro.Core.Models;

// Tells the system this blueprint belongs to the "users" table in the database
[Table("users")]
public class User : BaseModel
{
    // A unique identification code generated automatically for every person
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    // The name the user chooses to go by on the site
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    // The contact email address used for login and notifications
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    // Determines what the user is allowed to do, like "User" or "King"
    [Column("role")]
    public string Role { get; set; } = "User";

    // Stores the user's password in a scrambled, secure format
    [Column("password")]
    public string PasswordHash { get; set; } = string.Empty;

    // The person's legal first name
    [Column("first_name")]
    public string? FirstName { get; set; }

    // The person's legal last name
    [Column("last_name")]
    public string? LastName { get; set; }

    // The professional job title, such as "Full Stack Developer"
    [Column("Title")]
    public string? Title { get; set; }

    // A short paragraph describing the person's background and interests
    [Column("Bio")]
    public string? Bio { get; set; }

    // A count of how many years the person has worked in their field
    [Column("YearsOfExperience")]
    public int YearsOfExperience { get; set; }

    // A web link to the user's uploaded profile picture
    [Column("ProfileImageUrl")]
    public string? ProfileImageUrl { get; set; }

    // A web link to the user's uploaded resume document
    [Column("ResumeUrl")]
    public string? ResumeUrl { get; set; }

    // Short, catchy phrases used for headlines on the portfolio
    [Column("Tagline1")]
    public string? Tagline1 { get; set; }

    // A secondary headline for extra professional detail
    [Column("Tagline2")]
    public string? Tagline2 { get; set; }

    // A list of tools used for building the parts of a website users see
    [Column("FrontendSkills")]
    public string[]? FrontendSkills { get; set; } = Array.Empty<string>();

    // A list of tools used for building the "under the hood" logic
    [Column("BackendSkills")]
    public string[]? BackendSkills { get; set; } = Array.Empty<string>();

    // A list of tools used for managing and storing information
    [Column("DatabaseSkills")]
    public string[]? DatabaseSkills { get; set; } = Array.Empty<string>();

    // A link to the user's Instagram profile
    [Column("instagram_link")]
    public string? InstagramLink { get; set; }

    // A link to the user's GitHub profile where they store code
    [Column("GitHubLink")]
    public string? GitHubLink { get; set; }

    // A link to the user's professional LinkedIn profile
    [Column("linkedin_link")]
    public string? LinkedInLink { get; set; }

    // A temporary security code used to change a forgotten password
    [Column("reset_code")]
    public string? ResetCode { get; set; }

    // The exact time when the temporary security code will stop working
    [Column("reset_expiry")]
    public DateTime? ResetExpiry { get; set; }

    // The date and time when this account was first created
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
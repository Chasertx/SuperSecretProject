namespace PortfolioPro.Core.DTOs;

public class UserRegisterDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public int YearsOfExperience { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? ResumeUrl { get; set; }
    public string? Tagline1 { get; set; }
    public string? Tagline2 { get; set; }
    public string[]? FrontendSkills { get; set; }
    public string[]? BackendSkills { get; set; } 
    public string[]? DatabaseSkills { get; set; } 
    public string? InstagramLink { get; set; }
    public string? GitHubLink { get; set; }
    public string? LinkedInLink { get; set; }
    public string? FirstName { get; set; } // Added
    public string? LastName { get; set; }
}
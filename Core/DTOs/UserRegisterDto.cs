namespace PortfolioPro.Core.DTOs;

public class UserRegisterDto
{
    // The unique name the person chooses for logging in and their profile link
    public required string Username { get; set; }

    // The email address where the system sends notifications and password resets
    public required string Email { get; set; }

    // The secret password the person chooses to keep their account private
    public required string Password { get; set; }

    // The person's professional title, like "Lead Engineer" or "Graphic Designer"
    public string? Title { get; set; }

    // A descriptive paragraph about the person's career and background
    public string? Bio { get; set; }

    // A simple count of how long the person has been working professionally
    public int YearsOfExperience { get; set; }

    // A web link to the photo the user wants to show on their profile
    public string? ProfileImageUrl { get; set; }

    // A web link to the user's uploaded professional resume document
    public string? ResumeUrl { get; set; }

    // A short, punchy sentence used as a headline on the portfolio site
    public string? Tagline1 { get; set; }

    // A secondary headline for providing extra professional context
    public string? Tagline2 { get; set; }

    // A collection of technical tools used for the visual parts of a website
    public string[]? FrontendSkills { get; set; }

    // A collection of technical tools used for the background logic of a website
    public string[]? BackendSkills { get; set; }

    // A collection of tools used for organizing and storing data
    public string[]? DatabaseSkills { get; set; }

    // A web link to the user's personal or business Instagram page
    public string? InstagramLink { get; set; }

    // A web link to where the user hosts their computer code projects
    public string? GitHubLink { get; set; }

    // A web link to the person's professional networking profile
    public string? LinkedInLink { get; set; }

    // The person's legal first name
    public string? FirstName { get; set; }

    // The person's legal last name
    public string? LastName { get; set; }
}
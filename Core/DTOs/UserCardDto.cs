namespace PortfolioPro.Core.DTOs;

public class UserCardDto
{
    // The unique ID used to find the right person when someone clicks on their card
    public Guid Id { get; set; }

    // The display name shown on the user's public profile card
    public string Username { get; set; } = string.Empty;

    // The person's first name, used for a more personal greeting on the card
    public string? FirstName { get; set; }

    // The person's last name to complete their full professional name
    public string? LastName { get; set; }

    // The professional title (like "Web Designer") shown directly under their name
    public string? Title { get; set; }

    // A very short snippet of their bio to give a quick "first impression"
    public string? Bio { get; set; }

    // The link to the small square photo shown on the preview card
    public string? ProfileImageUrl { get; set; }

    // A small list of visual-building skills to highlight on the card
    public string[]? FrontendSkills { get; set; }

    // A small list of technical-logic skills to highlight on the card
    public string[]? BackendSkills { get; set; }
}
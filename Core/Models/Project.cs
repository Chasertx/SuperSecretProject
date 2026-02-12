namespace PortfolioPro.Core.Models;

public class Project
{
    // A unique identification code that tracks this specific project in the database
    public Guid Id { get; set; }

    // A link back to the specific user who created and owns this project
    public Guid UserId { get; set; }

    // The main name or headline of the project that people will see first
    public required string Title { get; set; }

    // The web address for the screenshot or cover photo of the work
    public string? ImageUrl { get; set; }

    // A summary that explains what the project is and how it was built
    public string Description { get; set; } = string.Empty;

    // A link to the source code or the main website for the project
    public string? ProjectUrl { get; set; }

    // A web link where people can click to see the project running live
    public string? LiveDemoURL { get; set; } = string.Empty;

    // The exact date and time the project was added to the portfolio
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
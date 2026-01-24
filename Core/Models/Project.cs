namespace PortfolioPro.Core.Models;
/**Model for projects.**/
public class Project
{
    // Unique identifier and primary key.
    public Guid Id { get; set; }

    // Unique foreign key linking project to owner.
    public Guid UserId { get; set; }

    // Name/Headline of the project. (Required)
    public required string Title { get; set; }

    // Url for the project's cover image.
    public string? ImageUrl { get; set; }

    // Brief description of the project's.
    public string Description { get; set; } = string.Empty;

    // Optional link to external project.
    public string? ProjectUrl { get; set; }

    // Timestamp of when the project was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
namespace PortfolioPro.Models;

public class Project
{
    // Primary Key
    public Guid Id { get; set; }

    // Foreign Key: Links this project to a specific User
    public Guid UserId { get; set; }

    // 'required' ensures the project isn't nameless
    public required string Title { get; set; }

    public string Description { get; set; } = string.Empty;

    // Nullable because not every project has a live link
    public string? ProjectUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
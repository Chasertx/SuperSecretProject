namespace PortfolioPro.Models;
/**Model for projects.**/
public class Project
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Title { get; set; }

    public string? ImageUrl { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? ProjectUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
namespace PortfolioPro.Core.DTOs;

public class UserCardDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string[]? FrontendSkills { get; set; }
    public string[]? BackendSkills { get; set; }
}
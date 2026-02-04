public class ProjectUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ProjectUrl { get; set; }
    public IFormFile? Image { get; set; } = null!;
    public string? LiveDemoURL { get; set; } = string.Empty;

}
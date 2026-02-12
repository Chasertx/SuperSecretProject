namespace PortfolioPro.Interfaces;

public interface IStorageService
{
    // Rule: Send a picture to a specific "folder" (bucket) in the cloud and get back a web link to it
    Task<string> UploadImageAsync(IFormFile file, string bucketName);

    // Rule: Send a picture to the default cloud storage area and get back a web link to it
    Task<string> UploadImageAsync(IFormFile file);

    // Rule: Find a specific picture using its web link and permanently erase it from the cloud
    Task DeleteImageAsync(string imageUrl);
}
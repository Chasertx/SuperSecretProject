using System.Net.Http.Headers;
using PortfolioPro.Interfaces;

namespace PortfolioPro.Services;
/*Do you have a repository of memes
you'd like to attach to your projects.
I sure do and this lets me do it. */

/// <summary>
/// Service responsible for managing
/// file uploads specifically to Supabase
/// storage buckets.
/// </summary>
public class sbstorageService(IConfiguration configuration, IHttpClientFactory httpClientFactory) : IStorageService
{
    /// <summary>
    /// Uploads an image file to supabase and 
    /// returns the public url for access.
    /// </summary>
    /// <param name="file">The file data provided by the HTTP request form.</param>
    /// <returns>A string representing the publicly accessible link to the uploaded image.</returns>
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        // Retrieves the base Supabase URL from appsettings.json.
        var supabaseUrl = configuration["Supabase:Url"];
        // Retrieves the service API key required for authorization.
        var apiKey = configuration["Supabase:ApiKey"];
        // Retrieves the specific folder/bucket name where images are stored.
        var bucket = configuration["Supabase:BucketName"];

        // Generates a unique filename using a GUID to prevent overwriting files with the same name.
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        // Constructs the specific API endpoint for uploading an object to the bucket.
        var uploadUrl = $"{supabaseUrl}/storage/v1/object/{bucket}/{fileName}";

        // Requests a managed HttpClient instance from the factory for better performance.
        var httpClient = httpClientFactory.CreateClient();
        // Creates a new POST request to the subabase storage endpoint.
        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        // Adds the required Supabase API key to the request headers.
        request.Headers.Add("apikey", apiKey);
        // Adds the bearer token (using the api key) for authenticated access.
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        // Opens a readable stream from the uploaded file data.
        using var stream = file.OpenReadStream();
        // Wraps the stream in a content container suitable for an HTTP request.
        var content = new StreamContent(stream);
        // Sets the 'content-type' header to match the original file type (e.g. image/png)
        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        // Attaches the file content to the request body.
        request.Content = content;

        // Sends the request async and waits for the server's response.
        var response = await httpClient.SendAsync(request);
        // Throws an exception if the upload failed (e.g., 401 Unauthorized or 404 not found).
        response.EnsureSuccessStatusCode();

        // Constructs and returns the final public URL to view the image in a browser.
        return $"{supabaseUrl}/storage/v1/object/public/{bucket}/{fileName}";
    }

    /// <summary>
    /// For deleting the image from storage in the supabase bucket.
    /// </summary>
    /// <param name="imageUrl"></param>
    /// <returns></returns>
    public async Task DeleteImageAsync(string imageUrl)
    {
        // Retrieve Supabase configuration values from appsettings.json.
        var supabaseUrl = configuration["Supabase:Url"];
        var apiKey = configuration["Supabase:ApiKey"];
        var bucket = configuration["Supabase:BucketName"];

        // Define the URL segment that precedes the actual file path.
        var identifier = $"/public/{bucket}/";
        // Extract only the file name by splitting the full URL and taking the last part.
        var fileName = imageUrl.Split(identifier).Last();
        // Construct the internal API endpoint for object deletion.
        var deleteUrl = $"{supabaseUrl}/storage/v1/object/{bucket}/{fileName}";
        // Get a resilient HttpClient instance from the factory.
        var httpClient = httpClientFactory.CreateClient();
        // Initialize a new HTTP DELETE request for the specific file.
        using var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
        // Attach the required Supabase API key for authentication and routing.
        request.Headers.Add("apikey", apiKey);
        // Provide the bearer token to authorize the storage operation.
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        // Execute the deletion request asynchronously.
        var response = await httpClient.SendAsync(request);

        // Check if the deletion failed (e.g., file not found or unauthorized).
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Storage Delete Failed: {error}");
        }
    }
}
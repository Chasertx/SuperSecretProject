using System.Net.Http.Headers;
using PortfolioPro.Interfaces;

namespace PortfolioPro.Services;
/*Do you have a repository of memes
you'd like to attach to your projects.
I sure do and this lets me do it. */
public class sbstorageService(IConfiguration configuration, IHttpClientFactory httpClientFactory) : IStorageService
{
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        var supabaseUrl = configuration["Supabase:Url"];
        var apiKey = configuration["Supabase:ApiKey"];
        var bucket = configuration["Supabase:BucketName"];

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var uploadUrl = $"{supabaseUrl}/storage/v1/object/{bucket}/{fileName}";

        var httpClient = httpClientFactory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Add("apikey", apiKey);
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        using var stream = file.OpenReadStream();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        request.Content = content;

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return $"{supabaseUrl}/storage/v1/object/public/{bucket}/{fileName}";
    }
}
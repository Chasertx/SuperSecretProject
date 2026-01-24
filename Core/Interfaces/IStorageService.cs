namespace PortfolioPro.Interfaces;
/** Interface for putting pics from 
your ski trip on your profile. **/
public interface IStorageService
{
    /// <summary>
    /// Processes an image upload from an
    /// http request and stores it in the cloud.
    /// </summary>
    /// <param name="file">Data stream sent from the browser.</param>
    /// <returns></returns>
    Task<string> UploadImageAsync(IFormFile file);
}
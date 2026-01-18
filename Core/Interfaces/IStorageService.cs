namespace PortfolioPro.Interfaces;
/** Interface for putting pics from 
your ski trip on your profile. **/
public interface IStorageService
{
    Task<string> UploadImageAsync(IFormFile file);
}
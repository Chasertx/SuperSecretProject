namespace PortfolioPro.Repositories;

public interface IAuthRepository
{
    Task<string?> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string email, string password);
    Task<bool> ResetPasswordRequestAsync(string email);
}
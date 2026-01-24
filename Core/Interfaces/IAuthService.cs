//Namespace for the repository layer.
namespace PortfolioPro.Repositories;

public interface IAuthRepository
{
    /// <summary>
    /// Validates user credentials and returns a jwt token if successful.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<string?> LoginAsync(string email, string password);

    /// <summary>
    /// Creates a new user record in the database.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns>True if user is created successfully.</returns>
    Task<bool> RegisterAsync(string email, string password);

    /// <summary>
    /// Generates a password reset code.
    /// </summary>
    /// <param name="email"></param>
    /// <returns>True if Successful.</returns>
    Task<bool> ResetPasswordRequestAsync(string email);
}
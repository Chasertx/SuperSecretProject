namespace PortfolioPro.Repositories;

public interface IAuthRepository
{
    // Rule: Check if the email and password are correct; if they are, provide a secure digital "key" to enter the site
    Task<string?> LoginAsync(string email, string password);

    // Rule: Take a new user's email and password and save them as a brand new account in the database
    Task<bool> RegisterAsync(string email, string password);

    // Rule: Start the recovery process for a user who forgot their password by creating a special reset code
    Task<bool> ResetPasswordRequestAsync(string email);
}
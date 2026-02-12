namespace PortfolioPro.Repositories;

public interface IAuthRepositories
{
    // Rule: Take the user's email and a secret security code, then save them together so we can check them later
    Task<bool> StoreResetCodeAsync(string email, string code);

    // Rule: Check if the email and code match what we saved; if they do, replace the old password with the new one
    Task<bool> VerifyAndResetPasswordAsync(string email, string code, string newPassword);
}
namespace PortfolioPro.Repositories;

public interface IAuthRepositories
{
    /// <summary>
    /// Stores the reset code associated with the user's email.
    /// </summary>
    /// <param name="email">User's email.</param>
    /// <param name="code">Reset code.</param>
    /// <returns></returns>
    Task<bool> StoreResetCodeAsync(string email, string code);
    /// <summary>
    /// Verifies the reset code and updates the password.
    /// </summary>
    /// <param name="email">user email</param>
    /// <param name="code">reset code</param>
    /// <param name="newPassword">desired new password</param>
    /// <returns></returns>
    Task<bool> VerifyAndResetPasswordAsync(string email, string code, string newPassword);
}
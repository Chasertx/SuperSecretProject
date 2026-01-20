namespace PortfolioPro.Repositories;

public interface IAuthRepositories
{
    Task<bool> StoreResetCodeAsync(string email, string code);
    Task<bool> VerifyAndResetPasswordAsync(string email, string code, string newPassword);
}
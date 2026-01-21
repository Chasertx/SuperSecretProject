using PortfolioPro.Core.Models;
using Supabase;
using Postgrest;

/* Everyone likes an authenticated person. */
namespace PortfolioPro.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly Supabase.Client _supabase;

    public AuthRepository(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public Task<string?> LoginAsync(string email, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RegisterAsync(string email, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ResetPasswordRequestAsync(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> StoreResetCodeAsync(string email, string code)
    {
        var response = await _supabase.From<User>()
            .Where(x => x.Email == email)
            .Get();

        User? user = response.Model;

        if (user == null) return false;

        user.ResetCode = code;
        user.ResetExpiry = DateTime.UtcNow.AddMinutes(15);

        await _supabase.From<User>().Update(user);
        return true;
    }

    public async Task<bool> VerifyAndResetPasswordAsync(string email, string code, string newPassword)
    {
        var response = await _supabase.From<User>()
            .Where(u => u.Email == email)
            .Where(u => u.ResetCode == code)
            .Get();

        var user = response.Model;

        if (user == null || user.ResetExpiry < DateTime.UtcNow) return false;

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetCode = null;
        user.ResetExpiry = null;

        await _supabase.From<User>().Update(user);
        return true;
    }
}
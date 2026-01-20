using PortfolioPro.Core.Models;
using Supabase;
using Postgrest; // Required for filtering queries

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
        // 1. Specifically ask for a 'User' response
        var response = await _supabase.From<User>()
            .Where(x => x.Email == email)
            .Get();

        // 2. Use 'Models.User?' to tell the compiler exactly what this variable is
        User? user = response.Model;

        if (user == null) return false;

        // Now 'user' is guaranteed to be the User class from your Models
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

        // Check if user exists and code hasn't expired
        if (user == null || user.ResetExpiry < DateTime.UtcNow) return false;

        // Update password and clear the reset code
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetCode = null;
        user.ResetExpiry = null;

        await _supabase.From<User>().Update(user);
        return true;
    }
}
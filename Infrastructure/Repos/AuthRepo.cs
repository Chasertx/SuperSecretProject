using PortfolioPro.Core.Models;
using Supabase;
using Postgrest;

/* Everyone likes an authenticated person. */
namespace PortfolioPro.Repositories;

/// <summary>
/// Implements authentication persistence logic using the Supabase client.
/// </summary>
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

    /// <summary>
    /// Attaches a security code and expiration to a user record for password recovery.
    /// </summary>
    public async Task<bool> StoreResetCodeAsync(string email, string code)
    {
        // Query user table for a matching email.
        var response = await _supabase.From<User>()
            .Where(x => x.Email == email)
            .Get();

        User? user = response.Model;

        //Return false if no account exists for the email.
        if (user == null) return false;

        // Update user object with the new reset metadata.
        user.ResetCode = code;
        user.ResetExpiry = DateTime.UtcNow.AddMinutes(15);

        // Push the updated user record back to supabase.
        await _supabase.From<User>().Update(user);
        return true;
    }

    /// <summary>
    /// Validates the reset code and updates the password if the code is current.
    /// </summary>
    public async Task<bool> VerifyAndResetPasswordAsync(string email, string code, string newPassword)
    {
        // Search for a user matching both the email and the specific reset code.
        var response = await _supabase.From<User>()
            .Where(u => u.Email == email)
            .Where(u => u.ResetCode == code)
            .Get();

        var user = response.Model;

        // Fail if user is missing or if the reset code has expired.
        if (user == null || user.ResetExpiry < DateTime.UtcNow) return false;

        // Securely hash the new password and clear the used reset metadata.
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetCode = null;
        user.ResetExpiry = null;

        // Persist the changes to the database.
        await _supabase.From<User>().Update(user);
        return true;
    }
}
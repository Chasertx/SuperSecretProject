using BCrypt.Net;
/**
This is a hasher for passwords prior to being
stored in the database.
**/
namespace PortfolioPro.Helpers;

public static class PasswordHasher
{
    /// <summary>
    /// Converts a plain-text
    /// password into a salted hash with 
    /// a work factor of 12.
    /// </summary>
    /// <param name="password">Raw string password</param>
    /// <returns>A hash string for database storage.</returns>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Compares a plain text password against
    /// a stored hash to determine a match.
    /// </summary>
    /// <param name="password">Plain text password.</param>
    /// <param name="hash">Hashed password from the database.</param>
    /// <returns>True if the passwords match.</returns>
    public static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
    }
}
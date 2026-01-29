using System.ComponentModel.DataAnnotations;
using PortfolioPro.Core.Models;
using System.Security.Claims;
/** Interface for managing user 
data prior to processing or storage.
**/
namespace PortfolioPro.Interfaces;

public interface IUserRepository
{
    // Gets a specific user using there ID.
    Task<User?> GetUserByIdAsync(Guid id);

    // Get a user from the database based on their email.
    Task<User?> GetUserByEmailAsync(string email);

    // Gets a complete like of all users in the database.
    Task<IEnumerable<User>> GetAllUsersAsync();

    // Adds a new user to the database.
    Task<Guid> AddUserAsync(User user);

    // Updates existing user information.
    Task UpdateUserAsync(User user);

    // Permanently deletes a user from the database.
    Task DeleteUserAsync(Guid id);

    // Stores a temporary reset code and it's expiration time.
    Task UpdateResetCodeAsync(string email, string resetCode, DateTime expiry);

    // Validates the reset code and updates the user's password.
    Task<bool> ResetPasswordAsync(string email, string code, string hashedPassword);

    Task UpdateResumeUrlAsync(Guid userId, string url);

    Guid GetUserId(ClaimsPrincipal user);

    Task<bool> UpdateUserProfileAsync(User user);
}
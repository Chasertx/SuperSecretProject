using System.ComponentModel.DataAnnotations;
using PortfolioPro.Core.Models;
using System.Security.Claims;

namespace PortfolioPro.Interfaces;

public interface IUserRepository
{
    // Rule: Find a specific person's full profile using their unique ID number
    Task<User?> GetUserByIdAsync(Guid id);

    // Rule: Search the database for an account that matches a specific email address
    Task<User?> GetUserByEmailAsync(string email);

    // Rule: Provide a complete list of every person registered in the system
    Task<IEnumerable<User>> GetAllUsersAsync();

    // Rule: Take a new person's info and save it as a brand new account
    Task<Guid> AddUserAsync(User user);

    // Rule: Take existing account info and overwrite it with updated details
    Task UpdateUserAsync(User user);

    // Rule: Permanently erase a person's account and data from the database
    Task DeleteUserAsync(Guid id);

    // Rule: Save a temporary "forgot password" code and record when it will stop working
    Task UpdateResetCodeAsync(string email, string resetCode, DateTime expiry);

    // Rule: Verify if a reset code is correct; if it is, save the person's new password
    Task<bool> ResetPasswordAsync(string email, string code, string hashedPassword);

    // Rule: Save the web link for where a user's resume file is stored online
    Task UpdateResumeUrlAsync(Guid userId, string url);

    // Rule: A helper to figure out the unique ID of the person currently logged in
    Guid GetUserId(ClaimsPrincipal user);

    // Rule: Specifically update the professional details (bio, skills, etc.) for a profile
    Task<bool> UpdateUserProfileAsync(User user);

    // Rule: Find a user based on their permissions, such as finding the site owner ("King")
    Task<User?> GetUserByRoleAsync(string role);

    // Rule: Update a specific web link for the site owner's assets, like their profile picture
    Task<bool> UpdateKingAssetUrlAsync(string bucketName, string url);

    // Rule: Send a file to the cloud and bring back the specific web address for it
    Task<string> GetSupabaseUrlAsync(IFormFile file, string bucketName);
}
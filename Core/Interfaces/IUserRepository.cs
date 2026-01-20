using System.ComponentModel.DataAnnotations;
using PortfolioPro.Core.Models;
/** Interface for managing user 
data prior to processing or storage.
**/
namespace PortfolioPro.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid id);

    Task<User?> GetUserByEmailAsync(string email);

    Task<IEnumerable<User>> GetAllUsersAsync();

    Task AddUserAsync(User user);

    Task UpdateUserAsync(User user);

    Task DeleteUserAsync(Guid id);

    Task UpdateResetCodeAsync(string email, string resetCode, DateTime expiry);
}
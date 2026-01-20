using System.Data;
using Dapper;
using PortfolioPro.Data;
using PortfolioPro.Core.Models;
using PortfolioPro.Interfaces;

namespace PortfolioPro.Repositories;
/**This is where the definitions of
all functionionality related to users
and their endpoints are defined. **/
public class UserRepository(DbConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        using var connection = connectionFactory.Create();
        const string sql = @"
            SELECT id, username, email, role, created_at AS CreatedAt, 
                   first_name AS FirstName, last_name AS LastName, password AS Password 
            FROM users 
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = connectionFactory.Create();
        const string sql = @"
            SELECT id, username, email, role, created_at AS CreatedAt, 
                   first_name AS FirstName, last_name AS LastName, password AS Password 
            FROM users 
            WHERE email = @Email";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        using var connection = connectionFactory.Create();
        const string sql = @"
            SELECT id, username, email, role, created_at AS CreatedAt, 
                   first_name AS FirstName, last_name AS LastName, password AS Password 
            FROM users";

        return await connection.QueryAsync<User>(sql);
    }

    public async Task AddUserAsync(User user)
    {
        using var connection = connectionFactory.Create();
        const string sql = @"
            INSERT INTO users (id, username, email, role, created_at, first_name, last_name, password) 
            VALUES (@Id, @Username, @Email, @Role, @CreatedAt, @FirstName, @LastName, @Password)";

        await connection.ExecuteAsync(sql, user);
    }

    public async Task UpdateUserAsync(User user)
    {
        using var connection = connectionFactory.Create();
        const string sql = @"
            UPDATE users 
            SET username = @Username, 
                email = @Email, 
                first_name = @FirstName, 
                last_name = @LastName, 
                role = @Role 
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        using var connection = connectionFactory.Create();
        const string sql = "DELETE FROM users WHERE id = @Id";

        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task UpdateResetCodeAsync(string email, string resetCode, DateTime expiry)
    {
        // 1. Create the connection
        using var connection = connectionFactory.Create();

        // 2. Define the SQL (PostgreSQL uses snake_case usually)
        // Make sure 'reset_code' and 'reset_expiry' match your Supabase column names!
        const string sql = @"
        UPDATE users 
        SET reset_code = @resetCode, 
            reset_expiry = @expiry 
        WHERE email = @email";

        // 3. Execute the command
        await connection.ExecuteAsync(sql, new
        {
            email,
            resetCode,
            expiry
        });
    }
}
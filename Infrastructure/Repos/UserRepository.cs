using System.Data;
using Dapper;
using PortfolioPro.Data;
using PortfolioPro.Core.Models;
using PortfolioPro.Interfaces;
using System.Security.Claims;

namespace PortfolioPro.Repositories;
/**BEHOLD! where I put all the stuff
that can mess with your stuff. More to
come. **/

/// <summary>
/// Manages user profile data and security credentials within the PostgreSQL database.
/// </summary>
public class UserRepository(DbConnectionFactory connectionFactory) : IUserRepository
{
    /// <summary>
    /// Finds a user by their unique identifier.
    /// </summary>
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        // Establish database connection.
        using var connection = connectionFactory.Create();

        // SQL with column aliasing to match C# property names.
        const string sql = @"
            SELECT id, username, email, role, created_at AS CreatedAt, 
                   first_name AS FirstName, last_name AS LastName, password AS Password 
            FROM users 
            WHERE id = @Id";

        // Query and return the first matching user or null.
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    /// <summary>
    /// Retrieves a user profile based on their unique email.
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
    SELECT 
        id, 
        username, 
        role, 
        first_name AS FirstName, 
        last_name AS LastName, 
        email, 
        password AS PasswordHash, -- This is the fix! Pointing 'password' to 'PasswordHash'
        reset_code AS ResetCode, 
        reset_expiry AS ResetExpiry,
        ""ProfileImageUrl"" AS ProfileImageUrl, 
        ""FrontendSkills"" AS FrontendSkills, 
        ""BackendSkills"" AS BackendSkills, 
        ""DatabaseSkills"" AS DatabaseSkills, 
        ""Bio"" AS Bio, 
        ""Title"" AS Title, 
        ""Tagline1"" AS Tagline1, 
        ""Tagline2"" AS Tagline2, 
        instagram_link AS InstagramLink, 
        ""GitHubLink"" AS GitHubLink, 
        linkedin_link AS LinkedinLink, 
        ""ResumeUrl"" AS ResumeUrl, 
        ""YearsOfExperience"" AS YearsOfExperience
    FROM users 
    WHERE email = @Email";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    /// <summary>
    /// Fetches all registered users in the system.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        using var connection = connectionFactory.Create();

        // We alias the snake_case and Quoted PascalCase columns 
        // to match your C# User.cs property names.
        const string sql = @"
        SELECT 
            id, 
            username, 
            email, 
            role, 
            created_at AS CreatedAt, 
            first_name AS FirstName, 
            last_name AS LastName,
            ""ProfileImageUrl"" AS ProfileImageUrl,
            ""Title"" AS Title,
            ""Bio"" AS Bio,
            ""FrontendSkills"" AS FrontendSkills,
            ""BackendSkills"" AS BackendSkills,
            ""Tagline1"" AS Tagline1
        FROM users 
        ORDER BY created_at DESC";

        return await connection.QueryAsync<User>(sql);
    }

    /// <summary>
    /// Creates a new user record in the database.
    /// </summary>
    public async Task<Guid> AddUserAsync(User user)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
        INSERT INTO users (
            id, username, role, created_at, first_name, last_name, 
            email, password, reset_code, reset_expiry,
            ""ProfileImageUrl"", ""FrontendSkills"", ""BackendSkills"", ""DatabaseSkills"", 
            ""Bio"", ""Title"", ""Tagline1"", ""Tagline2"", 
            instagram_link, ""GitHubLink"", linkedin_link, ""ResumeUrl"", 
            ""YearsOfExperience""
        ) 
        VALUES (
            @Id, @Username, @Role, @CreatedAt, @FirstName, @LastName, 
            @Email, @PasswordHash, @ResetCode, @ResetExpiry,
            @ProfileImageUrl, @FrontendSkills, @BackendSkills, @DatabaseSkills, 
            @Bio, @Title, @Tagline1, @Tagline2, 
            @InstagramLink, @GitHubLink, @LinkedinLink, @ResumeUrl, 
            @YearsOfExperience
        ) 
        RETURNING id;";

        // Dapper maps your C# 'PasswordHash' property to the '@PasswordHash' 
        // parameter, which inserts into the 'password' column.
        return await connection.ExecuteScalarAsync<Guid>(sql, user);
    }

    /// <summary>
    /// Updates an existing user's profile information.
    /// </summary>
    public async Task UpdateUserAsync(User user)
    {
        using var connection = connectionFactory.Create();

        // Update user-controlled fields excluding password.
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

    /// <summary>
    /// Permanently deletes a user account from the database.
    /// </summary>
    public async Task DeleteUserAsync(Guid id)
    {
        using var connection = connectionFactory.Create();

        // Execute physical row deletion.
        const string sql = "DELETE FROM users WHERE id = @Id";

        await connection.ExecuteAsync(sql, new { Id = id });
    }

    /// <summary>
    /// Saves a temporary reset code and expiration time for password recovery.
    /// </summary>
    public async Task UpdateResetCodeAsync(string email, string resetCode, DateTime expiry)
    {
        using var connection = connectionFactory.Create();

        // Store the reset token and it's validity window.
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

    /// <summary>
    /// Validates the reset code and updates the user's password with a new hash.
    /// </summary>
    public async Task<bool> ResetPasswordAsync(string email, string code, string hashedPassword)
    {
        using var connection = connectionFactory.Create();

        // Update password only if the code matches and hasn't expired
        const string sql = @"
            UPDATE users 
            SET password = @hashedPassword, 
                reset_code = NULL, 
                reset_expiry = NULL 
            WHERE email = @email 
            AND reset_code = @code 
            AND reset_expiry > CURRENT_TIMESTAMP";

        // Check rows affected to determine if the reset was successful.
        var rowsAffected = await connection.ExecuteAsync(sql, new { email, code, hashedPassword });

        return rowsAffected > 0;
    }

    public async Task UpdateResumeUrlAsync(Guid userId, string url)
    {
        using var connection = connectionFactory.Create();
        const string sql = @"
        UPDATE users 
        SET ""ResumeUrl"" = @Url 
        WHERE id = @UserId";

        await connection.ExecuteAsync(sql, new { UserId = userId, Url = url });
    }

    public Guid GetUserId(ClaimsPrincipal user)
    {
        // Look for the NameIdentifier claim (standard for IDs)
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? user.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

}

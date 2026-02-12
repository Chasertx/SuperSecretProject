using System.Data;
using Dapper;
using PortfolioPro.Data;
using PortfolioPro.Core.Models;
using PortfolioPro.Interfaces;
using System.Security.Claims;
using PortfolioPro.Services;

namespace PortfolioPro.Repositories;

public class UserRepository(DbConnectionFactory connectionFactory, IStorageService storageService) : IUserRepository
{
    // Fetches a user by their primary ID
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

    // Finds a user profile via email address
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
    SELECT 
        id, username, role, first_name AS FirstName, last_name AS LastName, email, 
        password AS PasswordHash, reset_code AS ResetCode, reset_expiry AS ResetExpiry,
        ""ProfileImageUrl"" AS ProfileImageUrl, ""FrontendSkills""::text[] AS FrontendSkills, 
        ""BackendSkills""::text[] AS BackendSkills, ""DatabaseSkills""::text[] AS DatabaseSkills,
        ""Bio"" AS Bio, ""Title"" AS Title, ""Tagline1"" AS Tagline1, ""Tagline2"" AS Tagline2, 
        instagram_link AS InstagramLink, ""GitHubLink"" AS GitHubLink, linkedin_link AS LinkedinLink, 
        ""ResumeUrl"" AS ResumeUrl, ""YearsOfExperience"" AS YearsOfExperience
    FROM users 
    WHERE email = @Email";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    // Pulls every registered user from the database
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
        SELECT 
            id, username, email, role, created_at AS CreatedAt, 
            first_name AS FirstName, last_name AS LastName,
            ""ProfileImageUrl"" AS ProfileImageUrl, ""Title"" AS Title,
            ""Bio"" AS Bio, ""FrontendSkills"" AS FrontendSkills,
            ""BackendSkills"" AS BackendSkills, ""Tagline1"" AS Tagline1
        FROM users 
        ORDER BY created_at DESC";

        return await connection.QueryAsync<User>(sql);
    }

    // Injects a new user record into the table
    public async Task<Guid> AddUserAsync(User user)
    {
        using var connection = connectionFactory.Create();
        Console.WriteLine($"Registering new user: {user.Username}");

        const string sql = @"
        INSERT INTO users (
            id, username, role, created_at, first_name, last_name, 
            email, password, reset_code, reset_expiry,
            ""ProfileImageUrl"", ""FrontendSkills"", ""BackendSkills"", ""DatabaseSkills"", 
            ""Bio"", ""Title"", ""Tagline1"", ""Tagline2"", 
            instagram_link, ""GitHubLink"", linkedin_link, ""ResumeUrl"", ""YearsOfExperience""
        ) 
        VALUES (
            @Id, @Username, @Role, @CreatedAt, @FirstName, @LastName, 
            @Email, @PasswordHash, @ResetCode, @ResetExpiry,
            @ProfileImageUrl, @FrontendSkills, @BackendSkills, @DatabaseSkills, 
            @Bio, @Title, @Tagline1, @Tagline2, 
            @InstagramLink, @GitHubLink, @LinkedinLink, @ResumeUrl, @YearsOfExperience
        ) 
        RETURNING id;";

        return await connection.ExecuteScalarAsync<Guid>(sql, user);
    }

    // Updates basic account identification fields
    public async Task UpdateUserAsync(User user)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
            UPDATE users 
            SET username = @Username, email = @Email, first_name = @FirstName, 
                last_name = @LastName, role = @Role 
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, user);
    }

    // Removes a user row permanently
    public async Task DeleteUserAsync(Guid id)
    {
        using var connection = connectionFactory.Create();
        const string sql = "DELETE FROM users WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    // Sets a temporary code for password recovery
    public async Task UpdateResetCodeAsync(string email, string resetCode, DateTime expiry)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
        UPDATE users 
        SET reset_code = @resetCode, reset_expiry = @expiry 
        WHERE email = @email";

        await connection.ExecuteAsync(sql, new { email, resetCode, expiry });
    }

    // Overwrites password if the reset code is valid and active
    public async Task<bool> ResetPasswordAsync(string email, string code, string hashedPassword)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
            UPDATE users 
            SET password = @hashedPassword, reset_code = NULL, reset_expiry = NULL 
            WHERE email = @email AND reset_code = @code AND reset_expiry > CURRENT_TIMESTAMP";

        var rowsAffected = await connection.ExecuteAsync(sql, new { email, code, hashedPassword });
        return rowsAffected > 0;
    }

    // Updates the path to the user's resume file
    public async Task UpdateResumeUrlAsync(Guid userId, string url)
    {
        using var connection = connectionFactory.Create();
        const string sql = @"UPDATE users SET ""ResumeUrl"" = @Url WHERE id = @UserId";
        await connection.ExecuteAsync(sql, new { UserId = userId, Url = url });
    }

    // Smart update that only touches fields that aren't null
    public async Task<bool> UpdateUserProfileAsync(User user)
    {
        using var connection = connectionFactory.Create();
        Console.WriteLine($"Syncing profile updates for user ID: {user.Id}");

        const string sql = @"
        UPDATE users 
        SET 
            first_name = COALESCE(@FirstName, first_name),
            last_name = COALESCE(@LastName, last_name),
            ""Title"" = COALESCE(@Title, ""Title""),
            ""Bio"" = COALESCE(@Bio, ""Bio""),
            ""Tagline1"" = COALESCE(@Tagline1, ""Tagline1""),
            ""Tagline2"" = COALESCE(@Tagline2, ""Tagline2""),
            ""ProfileImageUrl"" = COALESCE(@ProfileImageUrl, ""ProfileImageUrl""),
            ""FrontendSkills"" = COALESCE(@FrontendSkills::text[], ""FrontendSkills""::text[]),
            ""BackendSkills"" = COALESCE(@BackendSkills::text[], ""BackendSkills""::text[]),
            ""DatabaseSkills"" = COALESCE(@DatabaseSkills::text[], ""DatabaseSkills""::text[]),
            instagram_link = COALESCE(@InstagramLink, instagram_link),
            ""GitHubLink"" = COALESCE(@GitHubLink, ""GitHubLink""),
            linkedin_link = COALESCE(@LinkedinLink, linkedin_link),
            ""ResumeUrl"" = COALESCE(@ResumeUrl, ""ResumeUrl""),
            ""YearsOfExperience"" = CASE WHEN @YearsOfExperience = 0 THEN ""YearsOfExperience"" ELSE @YearsOfExperience END
        WHERE id = @Id;";

        var rowsAffected = await connection.ExecuteAsync(sql, user);
        return rowsAffected > 0;
    }

    // Extracts the Guid from the user's claims
    public Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    // Uploads a file to cloud storage
    public async Task<string> GetSupabaseUrlAsync(IFormFile file, string bucketName)
    {
        return await storageService.UploadImageAsync(file, bucketName);
    }

    // Updates specific assets for the site admin
    public async Task<bool> UpdateKingAssetUrlAsync(string bucketName, string url)
    {
        using var connection = connectionFactory.Create();

        string column = bucketName.ToLower() switch
        {
            "portfoliophoto" => @"""ProfileImageUrl""",
            "resumes" => @"""ResumeUrl""",
            _ => throw new ArgumentException("Invalid asset type")
        };

        string sql = $@"UPDATE users SET {column} = @Url WHERE role = 'King'";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Url = url });
        return rowsAffected > 0;
    }

    // Retrieves a user based on their assigned role
    public async Task<User?> GetUserByRoleAsync(string role)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
        SELECT 
            id, username, email, role, first_name, last_name, 
            ""Title"", ""Bio"", ""Tagline1"", ""Tagline2"",
            ""YearsOfExperience"", ""ProfileImageUrl"", ""ResumeUrl"",
            ""FrontendSkills"", ""BackendSkills"", ""DatabaseSkills"",
            ""GitHubLink"", instagram_link, linkedin_link
        FROM users 
        WHERE role = @Role LIMIT 1";

        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Role = role });
        if (row == null) return null;

        var dict = (IDictionary<string, object>)row;

        return new User
        {
            Id = dict.ContainsKey("id") ? (Guid)dict["id"] : Guid.Empty,
            Username = dict["username"]?.ToString() ?? "",
            Email = dict["email"]?.ToString() ?? "",
            Role = dict["role"]?.ToString() ?? "",
            FirstName = dict["first_name"]?.ToString(),
            LastName = dict["last_name"]?.ToString(),
            Title = dict["Title"]?.ToString(),
            Bio = dict["Bio"]?.ToString(),
            Tagline1 = dict["Tagline1"]?.ToString(),
            Tagline2 = dict["Tagline2"]?.ToString(),
            YearsOfExperience = dict.ContainsKey("YearsOfExperience") ? Convert.ToInt32(dict["YearsOfExperience"]) : 0,
            ProfileImageUrl = dict["ProfileImageUrl"]?.ToString(),
            ResumeUrl = dict["ResumeUrl"]?.ToString(),
            GitHubLink = dict["GitHubLink"]?.ToString(),
            InstagramLink = dict["instagram_link"]?.ToString(),
            LinkedInLink = dict["linkedin_link"]?.ToString(),
            FrontendSkills = MapArray(dict["FrontendSkills"]),
            BackendSkills = MapArray(dict["BackendSkills"]),
            DatabaseSkills = MapArray(dict["DatabaseSkills"])
        };
    }

    // Helper to safely convert database array formats to C# strings
    private string[] MapArray(object? val)
    {
        if (val == null) return Array.Empty<string>();
        if (val is string[] s) return s;
        if (val is IEnumerable<string> e) return e.ToArray();

        var str = val.ToString()?.Trim();
        if (!string.IsNullOrEmpty(str) && str.StartsWith("{") && str.EndsWith("}"))
        {
            return str.Trim('{', '}')
                      .Split(',')
                      .Select(x => x.Trim('"'))
                      .ToArray();
        }

        return Array.Empty<string>();
    }
}
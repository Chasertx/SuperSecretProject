using System.Data;
using Dapper;
using PortfolioPro.Data;
using PortfolioPro.Core.Models;
using PortfolioPro.Interfaces;

namespace PortfolioPro.Repositories;

/**
 * This is where the definitions of 
 * all functionality related to projects 
 * and their endpoints are defined. 
 **/

/// <summary>
/// Handles database operations for projects using dapper and raw SQL queries.
/// </summary>
public class ProjectRepository(DbConnectionFactory connectionFactory) : IProjectRepository
{
    /// <summary>
    /// Retrieves a single project by it's unique identifier.
    /// </summary>
    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        // Open a new connection via the factory.
        using var connection = connectionFactory.Create();

        // SQL query with aliases to map snake_case to Pascal case.
        const string sql = @"
            SELECT id, user_id AS UserId, title, description, image_url AS ImageUrl, project_url AS ProjectUrl, created_at AS CreatedAt 
            FROM projects 
            WHERE id = @Id";

        // Execute query and return the first result or null.
        return await connection.QueryFirstOrDefaultAsync<Project>(sql, new { Id = id });
    }

    /// <summary>
    /// Returns all active (non-deleted) projects for a specific user.
    /// </summary>
    public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(Guid userId)
    {
        using var connection = connectionFactory.Create();

        // Filters out projects that have a deleted_at timestamp
        const string sql = @"
            SELECT id, user_id AS UserId, title, description, image_url AS ImageUrl, project_url AS ProjectUrl, created_at AS CreatedAt 
            FROM projects 
            WHERE user_id = @UserId 
            AND deleted_at IS NULL";

        // Return a collection of matching projects.
        return await connection.QueryAsync<Project>(sql, new { UserId = userId });
    }

    /// <summary>
    /// Inserts a new project record into the database.
    /// </summary>
    public async Task AddProjectAsync(Project project)
    {
        using var connection = connectionFactory.Create();

        // Maps object properties directly to SQL parameters.
        const string sql = @"
            INSERT INTO projects (id, user_id, title, description, image_url, project_url, created_at) 
            VALUES (@Id, @UserId, @Title, @Description, @ImageUrl, @ProjectUrl, NOW())";

        // Execute the insert command.
        await connection.ExecuteAsync(sql, project);
    }

    /// <summary>
    /// Retrieves every project in the table regardless of owner or status.
    /// </summary>
    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        using var connection = connectionFactory.Create();
        //simple select query for all rows.
        return await connection.QueryAsync<Project>("SELECT * FROM projects");
    }

    /// <summary>
    /// Updates the text-based details and links of an existing project.
    /// </summary>
    public async Task UpdateProjectAsync(Project project)
    {
        using var connection = connectionFactory.Create();

        // Update specific fields based on the project ID.
        const string sql = @"
            UPDATE projects 
            SET title = @Title, 
                description = @Description, 
                project_url = @ProjectUrl 
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, project);
    }

    /// <summary>
    /// Hard deletes a project from the database permanently.
    /// </summary>
    public async Task DeleteProjectAsync(Guid id)
    {
        using var connection = connectionFactory.Create();

        // Execute a permanent row deletion.
        const string sql = "DELETE FROM projects WHERE id = @Id";

        await connection.ExecuteAsync(sql, new { Id = id });
    }

    /// <summary>
    /// Moves a project to the trash by setting a deletion timestamp.
    /// </summary>
    public async Task<bool> SoftDeleteProjectAsync(Guid projectId, Guid userId)
    {
        using var connection = connectionFactory.Create();

        // Sets the deleted_at flag to prevent showing in main lists.
        const string sql = @"
            UPDATE projects
            SET deleted_at = CURRENT_TIMESTAMP
            WHERE id = @projectId and user_id = @userId";

        // Check if the update actually affected a row (verifies ownership).
        var rowsAffected = await connection.ExecuteAsync(sql, new { projectId, userId });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Alias for GetProjectsByUserIdAsync to fetch active projects.
    /// </summary>    
    public async Task<IEnumerable<Project>> GetProjectsFromUserIdAsync(Guid userId)
    {
        using var connection = connectionFactory.Create();

        // Fetch only active records belonging to the user.
        const string sql = @"
        SELECT * FROM projects
        WHERE user_id = @userId
        AND deleted_at is Null
        ";

        return await connection.QueryAsync<Project>(sql, new { userId });
    }

    /// <summary>
    /// Recovers a project from the trash by clearing its deletion timestamp.
    /// </summary>
    public async Task<bool> RestoreProjectAsync(Guid projectId, Guid userId)
    {
        using var connection = connectionFactory.Create();

        // Clears the deleted_at field to make the project active again.
        const string sql = @"
        UPDATE projects
        SET deleted_at = NULL
        WHERE id = @projectId 
        AND user_id = @userId 
        AND deleted_at IS NOT NULL";

        // Returns true if a project was found in the trash and restored.
        var rowsAffected = await connection.ExecuteAsync(sql, new { projectId, userId });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Retrieves all projects that have been soft-deleted for a user. (Gets trash bin)
    /// </summary>
    public async Task<IEnumerable<Project>> GetDeletedProjectsAsync(Guid userId)
    {
        // Selects only trashed items ordered by most recently deleted.
        const string sql = @"
        SELECT * FROM projects
        WHERE user_id = @userId
        AND deleted_at IS NOT NULL
        order by deleted_at DESC
        ";

        // Create connection and query trashed items.
        return await connectionFactory.Create().QueryAsync<Project>(sql, new { userId });
    }

    // Gets image_url path to ensure the image is deleted from storage.
    public async Task<string?> GetImagePathAsync(Guid userId, Guid id)
    {
        // SQL to select the image_url from the projects table.
        const string sql = @"select image_url from projects where user_id = @userId and id = @id";

        // Establishes a connection to the database.
        using var connection = connectionFactory.Create();

        // Executes sql query on the database.
        return await connection.QuerySingleOrDefaultAsync<string?>(sql, new { id, userId });
    }

    // Deletes the project permanently from the repository.
    public async Task<bool> DeleteProjectAsync(Guid id, Guid userId)
    {
        // Deletes the intended project based on id and user id.
        const string sql = @"DELETE FROM projects WHERE id = @id AND user_id = @userId";

        // Establishest a connection.
        using var connection = connectionFactory.Create();

        // Uses that connection to execute our sql
        var rowsAffected = await connection.ExecuteAsync(sql, new { id, userId });

        // Checks if any rows were affected.
        return rowsAffected > 0;
    }

}
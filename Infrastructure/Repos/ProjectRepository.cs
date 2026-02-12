using System.Data;
using Dapper;
using PortfolioPro.Data;
using PortfolioPro.Core.Models;
using PortfolioPro.Interfaces;

namespace PortfolioPro.Repositories;

public class ProjectRepository(DbConnectionFactory connectionFactory) : IProjectRepository
{
    // Locates a single project using its unique ID
    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
        SELECT 
            id, user_id as UserId, title, description, 
            image_url as ImageUrl, project_url as ProjectUrl, 
            live_demo_url as LiveDemoURL, created_at as CreatedAt
        FROM projects 
        WHERE id = @Id";

        return await connection.QuerySingleOrDefaultAsync<Project>(sql, new { Id = id });
    }

    // Fetches all active projects belonging to a specific user
    public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(Guid userId)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
            SELECT id, user_id AS UserId, title, description, image_url AS ImageUrl, 
                   project_url AS ProjectUrl, live_demo_url AS LiveDemoUrl, created_at AS CreatedAt 
            FROM projects 
            WHERE user_id = @UserId AND deleted_at IS NULL";

        return await connection.QueryAsync<Project>(sql, new { UserId = userId });
    }

    // Records a brand new project into the database
    public async Task AddProjectAsync(Project project)
    {
        using var connection = connectionFactory.Create();
        Console.WriteLine($"Adding new project: {project.Title}");

        const string sql = @"
            INSERT INTO projects (id, user_id, title, description, image_url, project_url, live_demo_url, created_at) 
            VALUES (@Id, @UserId, @Title, @Description, @ImageUrl, @ProjectUrl, @LiveDemoUrl, NOW())";

        await connection.ExecuteAsync(sql, project);
    }

    // Returns every project record in the system
    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        using var connection = connectionFactory.Create();
        return await connection.QueryAsync<Project>("SELECT * FROM projects");
    }

    // Overwrites existing project details with updated info
    public async Task<bool> UpdateProjectAsync(Project project)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
        UPDATE projects 
        SET title = @Title, description = @Description, image_url = @ImageUrl, 
            project_url = @ProjectUrl, live_demo_url = @LiveDemoUrl
        WHERE id = @Id AND user_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, project);
        return rowsAffected > 0;
    }

    // Erases a project row permanently by its ID
    public async Task DeleteProjectAsync(Guid id)
    {
        using var connection = connectionFactory.Create();
        const string sql = "DELETE FROM projects WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    // Flags a project as deleted without removing the data
    public async Task<bool> SoftDeleteProjectAsync(Guid projectId, Guid userId)
    {
        using var connection = connectionFactory.Create();
        Console.WriteLine($"Moving project {projectId} to trash...");

        const string sql = @"
            UPDATE projects
            SET deleted_at = CURRENT_TIMESTAMP
            WHERE id = @projectId and user_id = @userId";

        var rowsAffected = await connection.ExecuteAsync(sql, new { projectId, userId });
        return rowsAffected > 0;
    }

    // Retrieves active projects for a user
    public async Task<IEnumerable<Project>> GetProjectsFromUserIdAsync(Guid userId)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
        SELECT * FROM projects
        WHERE user_id = @userId AND deleted_at is Null";

        return await connection.QueryAsync<Project>(sql, new { userId });
    }

    // Clears the deletion flag to bring a project back to life
    public async Task<bool> RestoreProjectAsync(Guid projectId, Guid userId)
    {
        using var connection = connectionFactory.Create();

        const string sql = @"
        UPDATE projects
        SET deleted_at = NULL
        WHERE id = @projectId AND user_id = @userId AND deleted_at IS NOT NULL";

        var rowsAffected = await connection.ExecuteAsync(sql, new { projectId, userId });
        return rowsAffected > 0;
    }

    // Lists all projects currently sitting in the trash
    public async Task<IEnumerable<Project>> GetDeletedProjectsAsync(Guid userId)
    {
        const string sql = @"
        SELECT * FROM projects
        WHERE user_id = @userId AND deleted_at IS NOT NULL
        ORDER BY deleted_at DESC";

        return await connectionFactory.Create().QueryAsync<Project>(sql, new { userId });
    }

    // Pulls the image path to help clean up file storage
    public async Task<string?> GetImagePathAsync(Guid userId, Guid id)
    {
        const string sql = @"SELECT image_url FROM projects WHERE user_id = @userId AND id = @id";
        using var connection = connectionFactory.Create();

        return await connection.QuerySingleOrDefaultAsync<string?>(sql, new { id, userId });
    }

    // Permanently removes a project if the user owns it
    public async Task<bool> DeleteProjectAsync(Guid id, Guid userId)
    {
        const string sql = @"DELETE FROM projects WHERE id = @id AND user_id = @userId";
        using var connection = connectionFactory.Create();

        var rowsAffected = await connection.ExecuteAsync(sql, new { id, userId });
        return rowsAffected > 0;
    }
}
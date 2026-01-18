using System.Data;
using Dapper;
using PortfolioPro.Data;
using PortfolioPro.Models;
using PortfolioPro.Interfaces;

namespace PortfolioPro.Repositories;

/**
 * This is where the definitions of 
 * all functionality related to projects 
 * and their endpoints are defined. 
 **/
public class ProjectRepository(DbConnectionFactory connectionFactory) : IProjectRepository
{
    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        using var connection = connectionFactory.Create();
        const string sql = @"
            SELECT id, user_id AS UserId, title, description, image_url AS ImageUrl, project_url AS ProjectUrl, created_at AS CreatedAt 
            FROM projects 
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Project>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(Guid userId)
    {
        using var connection = connectionFactory.Create();
        const string sql = @"
            SELECT id, user_id AS UserId, title, description, project_url AS ProjectUrl, created_at AS CreatedAt 
            FROM projects 
            WHERE user_id = @UserId";

        return await connection.QueryAsync<Project>(sql, new { UserId = userId });
    }

    public async Task AddProjectAsync(Project project)
    {
        using var connection = connectionFactory.Create();
        const string sql = @"
            INSERT INTO projects (id, user_id, title, description, image_url, project_url, created_at) 
            VALUES (@Id, @UserId, @Title, @Description, @ImageUrl, @ProjectUrl, NOW())";

        await connection.ExecuteAsync(sql, project);
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        using var connection = connectionFactory.Create();
        return await connection.QueryAsync<Project>("SELECT * FROM projects");
    }

    public async Task UpdateProjectAsync(Project project)
    {
        using var connection = connectionFactory.Create();
        const string sql = @"
            UPDATE projects 
            SET title = @Title, 
                description = @Description, 
                project_url = @ProjectUrl 
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, project);
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        using var connection = connectionFactory.Create();
        const string sql = "DELETE FROM projects WHERE id = @Id";

        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
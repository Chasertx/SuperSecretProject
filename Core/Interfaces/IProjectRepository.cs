using PortfolioPro.Core.Models;
/** This is an interface for
managing all project data prior 
to storage in the database.
**/
namespace PortfolioPro.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetProjectByIdAsync(Guid id);

    Task<IEnumerable<Project>> GetProjectsByUserIdAsync(Guid userId);

    Task AddProjectAsync(Project project);

    Task UpdateProjectAsync(Project project);

    Task DeleteProjectAsync(Guid id);
}
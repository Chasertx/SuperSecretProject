using PortfolioPro.Core.Models;
/** This is an interface for
managing all project data prior 
to storage in the database.
**/
namespace PortfolioPro.Interfaces;

// Interface for managing project data.
public interface IProjectRepository
{
    // Gets a project by is unique ID.
    Task<Project?> GetProjectByIdAsync(Guid id);

    // Get all projects associated a specific user.
    Task<IEnumerable<Project>> GetProjectsByUserIdAsync(Guid userId);

    // Inserts a new project into the database.
    Task AddProjectAsync(Project project);

    //Updates an existing project.
    Task UpdateProjectAsync(Project project);

    //Deletes a project permanently.
    Task DeleteProjectAsync(Guid id);

    // Essentially moves a projecto a trash bin. Can still be restored.
    Task<bool> SoftDeleteProjectAsync(Guid projectId, Guid userId);

    // Restores a project that was moved to the trash bin.
    Task<bool> RestoreProjectAsync(Guid projectId, Guid userId);

    // Gets all projects in the user's trash bin.
    Task<IEnumerable<Project>> GetDeletedProjectsAsync(Guid userId);

}
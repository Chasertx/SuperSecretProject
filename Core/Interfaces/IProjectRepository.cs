using PortfolioPro.Core.Models;

namespace PortfolioPro.Interfaces;

public interface IProjectRepository
{
    // Rule: Look up the full details of one specific project using its unique ID number
    Task<Project?> GetProjectByIdAsync(Guid id);

    // Rule: Bring back a list of every project created by a specific person
    Task<IEnumerable<Project>> GetProjectsByUserIdAsync(Guid userId);

    // Rule: Take a new project and save it permanently into the database
    Task AddProjectAsync(Project project);

    // Rule: Find an existing project and save any new changes made to it
    Task<bool> UpdateProjectAsync(Project project);

    // Rule: Permanently erase a project from the system using its ID
    Task DeleteProjectAsync(Guid id);

    // Rule: Move a project to the "Trash" so it's hidden but not gone forever
    Task<bool> SoftDeleteProjectAsync(Guid projectId, Guid userId);

    // Rule: Take a project out of the "Trash" and put it back on the live site
    Task<bool> RestoreProjectAsync(Guid projectId, Guid userId);

    // Rule: Show a list of all items currently sitting in a user's "Trash" bin
    Task<IEnumerable<Project>> GetDeletedProjectsAsync(Guid userId);

    // Rule: Find the web link for a project's image so the system knows which file to clean up
    Task<string?> GetImagePathAsync(Guid id, Guid userId);

    // Rule: Permanently delete a project after checking that the user actually owns it
    Task<bool> DeleteProjectAsync(Guid id, Guid userId);
}
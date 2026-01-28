using FluentValidation;
using PortfolioPro.Core.Models;
using PortfolioPro.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PortfolioPro.Endpoints;

public static class ProjectEndpoints
{
    /* This allows you to define endpoints
    for all your projects. It doesn't have
    everything yet. But its got STUFF. */

    /// <summary>
    /// Registers project-related endpoints including CRUD and trash management.
    /// </summary>
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        // Creates a grouped route prefix for all project actions.
        var group = app.MapGroup("/api/projects");

        /// <summary>
        /// Retrieves all projects for a specific user by their unique ID.
        /// </summary>
        group.MapGet("/user/{userId:guid}", async (Guid userId, IProjectRepository repo) =>
        {
            // Fetch project list from database.
            var projects = await repo.GetProjectsByUserIdAsync(userId);
            // Return the list with a 200 OK status.
            return Results.Ok(projects);
        });

        /// <summary>
        /// Retrieves a new project with an image upload.
        /// </summary>
        group.MapPost("/", async (
            [FromForm] ProjectUploadRequest request,
            IStorageService storageService,
            IProjectRepository projectRepo,
            IValidator<Project> validator,
            HttpContext context) =>
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Results.Unauthorized();
            var userId = Guid.Parse(userIdClaim);

            // Use request.Image
            var imageUrl = await storageService.UploadImageAsync(request.Image);

            var newProject = new Project
            {
                Id = Guid.NewGuid(),
                Title = request.Title,       // Use request.Title
                Description = request.Description,
                ImageUrl = imageUrl,
                ProjectUrl = request.ProjectUrl,
                UserId = userId
            };

            var validationResult = await validator.ValidateAsync(newProject);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            await projectRepo.AddProjectAsync(newProject);

            return Results.Created($"/api/projects/{newProject.Id}", newProject);
        })
        .DisableAntiforgery()
        .RequireAuthorization();

        /// <summary>
        /// Gets all projects belonging to the currently logged-in user.
        /// </summary>
        group.MapGet("/my-projects", async (HttpContext context, IProjectRepository projectRepo) =>
        {

            // Retrieves and returns user-specific projects.
            var userId = Guid.Parse("802a9231 - 6482 - 42a3 - b5d1 - cbe3bf994034");
            var projects = await projectRepo.GetProjectsByUserIdAsync(userId);

            // Return the list of projects with a 200 OK status.
            return Results.Ok(projects);
        });

        /// <summary>
        /// Soft-deletes a project (moves it to trash bin).
        /// </summary>
        group.MapDelete("/{id:guid}", async (Guid id, IProjectRepository repo, HttpContext context) =>
        {
            // Ensure the user owns the project before deleting.
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Results.Unauthorized();

            // Performs the soft-delete operation on the database.
            var success = await repo.SoftDeleteProjectAsync(id, Guid.Parse(userIdClaim));

            // Returns no content on success, or not found if the project doesn't exist.
            return success ? Results.NoContent() : Results.NotFound("Project not found or already deleted.");
        })
        .RequireAuthorization();

        /// <summary>
        /// Retrieves all soft-deleted projects for the authenticated user. (gets trash bin)
        /// </summary>
        group.MapGet("/getDeleted", async (IProjectRepository repo, HttpContext context) =>
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Results.Unauthorized();

            var deletedProjects = await repo.GetDeletedProjectsAsync(Guid.Parse(userIdClaim));
            return Results.Ok(deletedProjects);
        });

        /// <summary>
        /// Restores a soft-deleted project back to active status.
        /// </summary>
        group.MapPatch("/{id:guid}/restore", async (Guid id, IProjectRepository repo, HttpContext context) =>
        {
            // Identify the user making the restore request.
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Results.Unauthorized();
            Console.WriteLine($"TOKEN ID=========================================================: {userIdClaim}");

            // Attempt to clear the deletion timestamp.
            var success = await repo.RestoreProjectAsync(id, Guid.Parse(userIdClaim));

            // Return OK or not found based on the operation result.
            return success
                ? Results.Ok("Project successfully restored.")
                : Results.NotFound("Project not found or is not currently deleted.");
        })
        .RequireAuthorization();

        /// <summary>
        /// Permanently deletes the a project from the repository.
        /// </summary>
        group.MapDelete("/{id:guid}/permanent", async (Guid id, IProjectRepository repo, IStorageService sb, ClaimsPrincipal User) =>
        {
            // Getting the identifier from the logged in user's claim
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // Getting the image path url 
            var imageUrl = await repo.GetImagePathAsync(id, userId);

            // Deleting the project 
            var success = await repo.DeleteProjectAsync(id, userId);
            if (!success) return Results.NotFound("Project not found or unauthorized.");

            // Checks if an image url was found
            if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    // Deletes the image from the supabase bucket
                    await sb.DeleteImageAsync(imageUrl);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the request since DB record is already gone
                    Console.WriteLine($"Orphaned file alert: {imageUrl}. Error: {ex.Message}");
                }
            }

            return Results.NoContent();
        });

    }
}
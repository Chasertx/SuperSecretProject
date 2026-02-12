using FluentValidation;
using PortfolioPro.Core.Models;
using PortfolioPro.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace PortfolioPro.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects");

        // GET /api/projects/{userId} - Asks the database for every project belonging to a specific person's ID
        group.MapGet("/{userId:guid}", async (Guid userId, IProjectRepository repo) =>
        {
            var projects = await repo.GetProjectsByUserIdAsync(userId);
            return Results.Ok(projects);
        });

        // PUT /api/projects/{id} - Finds a project, checks if the user owns it, and updates the text or image files
        group.MapPut("/{id:guid}", async (
           Guid id,
           [FromForm] ProjectUploadRequest request,
           IProjectRepository repo,
           IStorageService storage,
           HttpContext context) =>
        {
            var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var existing = await repo.GetProjectByIdAsync(id);
            if (existing == null || existing.UserId != userId) return Results.NotFound();

            if (request.Image != null)
            {
                existing.ImageUrl = await storage.UploadImageAsync(request.Image);
            }

            existing.Title = !string.IsNullOrWhiteSpace(request.Title) ? request.Title : existing.Title;
            existing.Description = !string.IsNullOrWhiteSpace(request.Description) ? request.Description : existing.Description;
            existing.ProjectUrl = request.ProjectUrl ?? existing.ProjectUrl;
            existing.LiveDemoURL = request.LiveDemoURL ?? existing.LiveDemoURL;

            if (request.Image != null && request.Image.Length > 0)
            {
                existing.ImageUrl = await storage.UploadImageAsync(request.Image);
            }

            await repo.UpdateProjectAsync(existing);
            return Results.Ok(existing);
        }).DisableAntiforgery()
         .RequireAuthorization();

        // GET /api/projects/storage/{id} - Internal tool to double-check a project's location in the storage system
        group.MapGet("/storage/{id:guid}", async (Guid id, IProjectRepository repository) =>
        {
            var project = await repository.GetProjectByIdAsync(id);

            return project is not null
                ? Results.Ok(project)
                : Results.NotFound(new { Message = $"Project {id} not found" });
        });

        // POST /api/projects - Takes new project info and a picture, verifies the data is valid, and saves it to the database
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

            var imageUrl = await storageService.UploadImageAsync(request.Image);

            var newProject = new Project
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                ImageUrl = imageUrl,
                ProjectUrl = request.ProjectUrl,
                UserId = userId,
                LiveDemoURL = request.LiveDemoURL
            };

            var validationResult = await validator.ValidateAsync(newProject);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            await projectRepo.AddProjectAsync(newProject);

            Console.WriteLine($"Project '{request.Title}' successfully published.");

            return Results.Created($"/api/projects/{newProject.Id}", newProject);
        })
        .DisableAntiforgery()
        .RequireAuthorization();

        // GET /api/projects/my-projects - Looks up the logged-in user's ID to show them only their own work
        group.MapGet("/my-projects", async (HttpContext context, IProjectRepository projectRepo) =>
        {
            var userId = Guid.Parse("802a9231-6482-42a3-b5d1-cbe3bf994034");
            var projects = await projectRepo.GetProjectsByUserIdAsync(userId);

            return Results.Ok(projects);
        });

        // DELETE /api/projects/{id} - Hides a project by moving it to the "Trash" instead of deleting it forever
        group.MapDelete("/{id:guid}", async (Guid id, IProjectRepository repo, HttpContext context) =>
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Results.Unauthorized();

            var success = await repo.SoftDeleteProjectAsync(id, Guid.Parse(userIdClaim));

            return success ? Results.NoContent() : Results.NotFound("Project not found or already deleted.");
        })
        .RequireAuthorization();

        // GET /api/projects/getDeleted - Shows the user a list of all items currently sitting in their "Trash" bin
        group.MapGet("/getDeleted", async (IProjectRepository repo, HttpContext context) =>
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Results.Unauthorized();

            var deletedProjects = await repo.GetDeletedProjectsAsync(Guid.Parse(userIdClaim));
            return Results.Ok(deletedProjects);
        });

        // PATCH /api/projects/{id}/restore - Takes a project out of the "Trash" and puts it back on the live site
        group.MapPatch("/{id:guid}/restore", async (Guid id, IProjectRepository repo, HttpContext context) =>
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Results.Unauthorized();

            var success = await repo.RestoreProjectAsync(id, Guid.Parse(userIdClaim));

            return success
                ? Results.Ok("Project successfully restored.")
                : Results.NotFound("Project not found or is not currently deleted.");
        })
        .RequireAuthorization();

        // DELETE /api/projects/{id}/permanent - Erases the project from the database and deletes its image file from the cloud
        group.MapDelete("/{id:guid}/permanent", async (Guid id, IProjectRepository repo, IStorageService sb, ClaimsPrincipal User) =>
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var imageUrl = await repo.GetImagePathAsync(id, userId);

            var success = await repo.DeleteProjectAsync(id, userId);
            if (!success) return Results.NotFound("Project not found or unauthorized.");

            if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    await sb.DeleteImageAsync(imageUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cloud storage cleanup failed for: {imageUrl}. Error: {ex.Message}");
                }
            }

            return Results.NoContent();
        });
    }
}
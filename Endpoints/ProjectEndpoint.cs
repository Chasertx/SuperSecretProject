using FluentValidation;
using PortfolioPro.Core.Models;
using PortfolioPro.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioPro.Endpoints;

public static class ProjectEndpoints
{
    /* This allows you to define endpoints
    for all your projects. It doesn't have
    everything yet. But its got STUFF. */
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects");

        group.MapGet("/user/{userId:guid}", async (Guid userId, IProjectRepository repo) =>
        {
            var projects = await repo.GetProjectsByUserIdAsync(userId);
            return Results.Ok(projects);
        });

        // --- POST Endpoints ---
        group.MapPost("/", async (
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] string? projectUrl,
            [FromForm] IFormFile image,
            IStorageService storageService,
            IProjectRepository projectRepo,
            IValidator<Project> validator,
            HttpContext context) =>
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Results.Unauthorized();
            var userId = Guid.Parse(userIdClaim);

            var imageUrl = await storageService.UploadImageAsync(image);

            var newProject = new Project
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                ImageUrl = imageUrl,
                ProjectUrl = projectUrl,
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

        group.MapGet("/my-projects", async (HttpContext context, IProjectRepository projectRepo) =>
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Results.Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var projects = await projectRepo.GetProjectsByUserIdAsync(userId);

            return Results.Ok(projects);
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, IProjectRepository repo) =>
        {
            var project = await repo.GetProjectByIdAsync(id);
            if (project is null)
                return Results.NotFound(new { Message = "Project not found" });

            await repo.DeleteProjectAsync(id);
            return Results.NoContent();
        })
        .RequireAuthorization();
    }
}
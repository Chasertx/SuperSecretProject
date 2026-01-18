using FluentValidation;
using PortfolioPro.Models;
using PortfolioPro.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioPro.Endpoints;

/** * This defines all api endpoints for 
 * managing project data. You can even add
 a picture of a wombat to your project
 since they're the coolest.
 **/
public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects");

        // --- GET Endpoints ---
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
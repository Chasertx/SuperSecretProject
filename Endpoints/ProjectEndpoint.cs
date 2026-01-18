using FluentValidation;
using PortfolioPro.Models;
using PortfolioPro.Interfaces;

namespace PortfolioPro.Endpoints;
/** This defines all api endpoints for
managing project data. **/
public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        // All routes in this file start with /api/projects
        var group = app.MapGroup("/api/projects");

        group.MapPost("/", async (Project project, IProjectRepository repo, IValidator<Project> validator) =>
        {
            var validationResult = await validator.ValidateAsync(project);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            project.Id = Guid.NewGuid();
            await repo.AddProjectAsync(project);

            return Results.Created($"/api/projects/{project.Id}", project);
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, IProjectRepository repo) =>
        {
            var project = await repo.GetProjectByIdAsync(id);
            if (project is null) return Results.NotFound(new { Message = "Project not found" });

            await repo.DeleteProjectAsync(id);
            return Results.NoContent();
        }).RequireAuthorization();

        group.MapGet("/user/{userId:guid}", async (Guid userId, IProjectRepository repo) =>
        {
            var projects = await repo.GetProjectsByUserIdAsync(userId);
            return Results.Ok(projects);
        });
    }
}
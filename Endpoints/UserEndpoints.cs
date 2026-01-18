using FluentValidation;
using PortfolioPro.Models;
using PortfolioPro.Helpers;
using PortfolioPro.Interfaces;

namespace PortfolioPro.Endpoints;
/** This defines all api endpoints for
managing user data. **/
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        group.MapPost("/register", async (User user, IUserRepository repo, IValidator<User> validator) =>
        {
            var validationResult = await validator.ValidateAsync(user);
            if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());

            user.Password = PasswordHasher.HashPassword(user.Password);
            user.Id = Guid.NewGuid();

            await repo.AddUserAsync(user);
            return Results.Created($"/api/users/{user.Id}", user);
        });

        group.MapPost("/login", async (LoginRequest login, IUserRepository repo, ITokenService tokenService) =>
        {
            var user = await repo.GetUserByEmailAsync(login.Username);

            if (user is null || !PasswordHasher.VerifyPassword(login.Password, user.Password))
            {
                return Results.Unauthorized();
            }

            var token = tokenService.CreateToken(user);
            return Results.Ok(new { Message = "Login Successful", Token = token, User = user });
        });

        group.MapGet("/", async (IUserRepository repo) =>
        {
            return Results.Ok(await repo.GetAllUsersAsync());
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            var user = await repo.GetUserByIdAsync(id);
            return user is not null ? Results.Ok(user) : Results.NotFound();
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            var user = await repo.GetUserByIdAsync(id);
            if (user is null) return Results.NotFound();

            await repo.DeleteUserAsync(id);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
using FluentValidation;
using PortfolioPro.Core.Models;
using PortfolioPro.Helpers;
using PortfolioPro.Interfaces;
using PortfolioPro.interfaces;

namespace PortfolioPro.Endpoints;
/** This is alot of stuff for messing
with your user profile. **/
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
            return Results.Created($"/api/users/{user.Id}", new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role
            });
        });

        group.MapPost("/login", async (LoginRequest login, IUserRepository repo, ITokenService tokenService) =>
            {
                Console.WriteLine($"Login attempt for Username/Email: '{login.Username}'");

                var user = await repo.GetUserByEmailAsync(login.Username);

                if (user is null)
                {
                    Console.WriteLine("User NOT found in database.");
                    return Results.Unauthorized();
                }

                Console.WriteLine($"User found. Database hash starts with: {user.Password.Substring(0, 5)}");


                bool isPasswordValid = PasswordHasher.VerifyPassword(login.Password, user.Password);
                Console.WriteLine($"Password verification result: {isPasswordValid}");

                if (!isPasswordValid)
                {
                    return Results.Unauthorized();
                }

                var token = tokenService.CreateToken(user);
                return Results.Ok(new { Message = "Login Successful", Token = token });
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

        group.MapPost("/forgot-password", async (string email, IUserRepository repo, IEmailService emailService) =>
        {
            var user = await repo.GetUserByEmailAsync(email);

            // 1. Safety check
            if (user == null) return Results.Ok("If an account exists, a code was sent.");

            // 2. Generate the code
            var resetCode = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(15);

            // 3. Save to DB (Requires UpdateResetCodeAsync in your repo)
            await repo.UpdateResetCodeAsync(email, resetCode, expiry);

            // 4. Send the email
            await emailService.SendEmailAsync(
                email,
                "Password Reset",
                $"Your code is {resetCode}"
            );

            return Results.Ok("Reset code sent.");
        });

        group.MapPost("/reset-password", async (ResetPasswordRequest request, IUserRepository repo) =>
        {
            // 1. (Optional but recommended) Hash the new password!
            // For now, we'll assume you have a hashing helper. 
            // If not, use the same hashing logic you used during Signup.
            var newHashedPassword = PasswordHasher.HashPassword(request.NewPassword);

            // 2. Try to update the password
            var success = await repo.ResetPasswordAsync(request.Email, request.Code, newHashedPassword);

            if (!success)
            {
                return Results.BadRequest("Invalid code, expired, or incorrect email.");
            }

            return Results.Ok("Password has been reset successfully.");
        });


    }

    public record ResetPasswordRequest(string Email, string Code, string NewPassword);
}
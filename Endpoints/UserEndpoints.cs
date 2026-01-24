using FluentValidation;
using PortfolioPro.Core.Models;
using PortfolioPro.Helpers;
using PortfolioPro.Interfaces;
using PortfolioPro.interfaces;

namespace PortfolioPro.Endpoints;
/** This is alot of stuff for messing
with your user profile. **/

/// <summary>
/// Configures and maps endpoints for user 
/// account management, authentication,
/// and security.
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Registers user-related routes for the application.
    /// </summary>
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // Groups all routs under the /api/users prefix.
        var group = app.MapGroup("/api/users");

        /// <summary>
        /// Registers a new user with a hashed password.
        /// </summary>
        group.MapPost("/register", async (User user, IUserRepository repo, IValidator<User> validator) =>
        {
            // Validate user input against business rules.
            var validationResult = await validator.ValidateAsync(user);
            if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());

            // Hash password before storage and generates unique ID.
            user.Password = PasswordHasher.HashPassword(user.Password);
            user.Id = Guid.NewGuid();

            // Save user to database and returns profile (excluding password).
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

        /// <summary>
        /// Authenticates a user and returns a JWT access token.
        /// </summary>
        group.MapPost("/login", async (LoginRequest login, IUserRepository repo, ITokenService tokenService) =>
            {
                Console.WriteLine($"Login attempt for Username/Email: '{login.Username}'");

                // Find user by their provided email/username.
                var user = await repo.GetUserByEmailAsync(login.Username);

                // Fail if user does not exist.
                if (user is null)
                {
                    Console.WriteLine("User NOT found in database.");
                    return Results.Unauthorized();
                }

                Console.WriteLine($"User found. Database hash starts with: {user.Password.Substring(0, 5)}");

                // Check if the provided password matches the stored hash.
                bool isPasswordValid = PasswordHasher.VerifyPassword(login.Password, user.Password);
                Console.WriteLine($"Password verification result: {isPasswordValid}");

                // Fail if password doesn't match.
                if (!isPasswordValid)
                {
                    return Results.Unauthorized();
                }

                // Generate and return security token for valid credentials
                var token = tokenService.CreateToken(user);
                return Results.Ok(new { Message = "Login Successful", Token = token });
            });


        /// <summary>
        /// Retrieves a list of all registered users.
        /// </summary>
        group.MapGet("/", async (IUserRepository repo) =>
        {
            // Fetch all records (Authorization required).
            return Results.Ok(await repo.GetAllUsersAsync());
        }).RequireAuthorization();

        /// <summary>
        /// Gets a specific user by their unique ID.
        /// </summary>
        group.MapGet("/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            // Attempt to find the user in the database
            var user = await repo.GetUserByIdAsync(id);

            // Return 200 if found, else 404 not found.
            return user is not null ? Results.Ok(user) : Results.NotFound();
        }).RequireAuthorization();

        /// <summary>
        /// Deletes a user account from the system.
        /// </summary>
        group.MapDelete("/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            // Check for user existence before attempting the delete.
            var user = await repo.GetUserByIdAsync(id);
            if (user is null) return Results.NotFound();

            // Perform permanent deletion.
            await repo.DeleteUserAsync(id);
            return Results.NoContent();
        }).RequireAuthorization();

        /// <summary>
        /// Triggers the password reset process by send a code to the user's email.
        /// </summary>
        group.MapPost("/forgot-password", async (string email, IUserRepository repo, IEmailService emailService) =>
        {
            // Search for user by email.
            var user = await repo.GetUserByEmailAsync(email);

            // Hide account existence to preven user enumeration.
            if (user == null) return Results.Ok("If an account exists, a code was sent.");

            // Generates a random 6 digit numeric reset code.
            var resetCode = new Random().Next(100000, 999999).ToString();
            // Set code expiry time for 15 minutes
            var expiry = DateTime.UtcNow.AddMinutes(15);

            // Persist the reset code and expiry to the user record.
            await repo.UpdateResetCodeAsync(email, resetCode, expiry);

            // Dispatch the email containing the reset code.
            await emailService.SendEmailAsync(
                email,
                "Password Reset",
                $"Your code is {resetCode}"
            );

            return Results.Ok("Reset code sent.");
        });

        /// <summary>
        /// Validates a reset code and updates the user's password.
        /// </summary>
        group.MapPost("/reset-password", async (ResetPasswordRequest request, IUserRepository repo) =>
        {
            // Hash the new password for secure storage.
            var newHashedPassword = PasswordHasher.HashPassword(request.NewPassword);

            // Validate code and update database in one operation.
            var success = await repo.ResetPasswordAsync(request.Email, request.Code, newHashedPassword);

            // Handle invalid or expired codes.
            if (!success)
            {
                return Results.BadRequest("Invalid code, expired, or incorrect email.");
            }

            return Results.Ok("Password has been reset successfully.");
        });


    }

    // Helper record for the password reset payload.
    public record ResetPasswordRequest(string Email, string Code, string NewPassword);
}
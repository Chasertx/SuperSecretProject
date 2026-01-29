using FluentValidation;
using PortfolioPro.Core.Models;
using PortfolioPro.Core.DTOs;
using PortfolioPro.Helpers;
using PortfolioPro.Interfaces;
using PortfolioPro.interfaces;
using PortfolioPro;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioPro.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        // --- REGISTER ---
        group.MapPost("/register", async (UserRegisterDto request, IUserRepository repo, IValidator<UserRegisterDto> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,

                // Use the BCrypt helper we set up earlier
                PasswordHash = PasswordHasher.HashPassword(request.Password),

                Title = request.Title,
                Bio = request.Bio,
                YearsOfExperience = request.YearsOfExperience,
                ProfileImageUrl = request.ProfileImageUrl,
                ResumeUrl = request.ResumeUrl,
                Tagline1 = request.Tagline1,
                Tagline2 = request.Tagline2,
                FrontendSkills = request.FrontendSkills?.ToArray(), // Convert List to string[]
                BackendSkills = request.BackendSkills?.ToArray(),   // Convert List to string[]
                DatabaseSkills = request.DatabaseSkills?.ToArray(),  // Convert List to string[]
                InstagramLink = request.InstagramLink,
                GitHubLink = request.GitHubLink,
                LinkedInLink = request.LinkedInLink,
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddUserAsync(user);

            return Results.Created($"/api/users/{user.Id}", new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Title
            });
        });

        // --- LOGIN ---
        group.MapPost("/login", async (LoginRequest login, IUserRepository repo, ITokenService tokenService) =>
        {
            var user = await repo.GetUserByEmailAsync(login.Email);

            if (user is null) return Results.Unauthorized();

            bool isPasswordValid = PasswordHasher.VerifyPassword(login.Password, user.PasswordHash);

            if (!isPasswordValid) return Results.Unauthorized();

            var token = tokenService.CreateToken(user);

            // Return the token plus the basic info requested
            return Results.Ok(new
            {
                Message = "Login Successful",
                Token = token,
                User = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Username,
                    user.Role // Added role too, as it's usually needed for frontend routing
                }
            });
        });
        // --- GET ALL ---
        group.MapGet("/", async (IUserRepository repo) =>
        {
            var users = await repo.GetAllUsersAsync();

            // Transform the 'User' models into 'UserCardDto' objects
            var userCards = users.Select(u => new UserCardDto
            {
                Id = u.Id,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Title = u.Title,
                Bio = u.Bio,
                ProfileImageUrl = u.ProfileImageUrl,
                FrontendSkills = u.FrontendSkills?.ToArray(),
                BackendSkills = u.BackendSkills?.ToArray()
            });

            return Results.Ok(userCards);
        });

        // --- GET BY ID ---
        group.MapGet("/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            var user = await repo.GetUserByIdAsync(id);
            return user is not null ? Results.Ok(user) : Results.NotFound();
        }).RequireAuthorization();

        // --- DELETE ---
        group.MapDelete("/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            var user = await repo.GetUserByIdAsync(id);
            if (user is null) return Results.NotFound();

            await repo.DeleteUserAsync(id);
            return Results.NoContent();
        }).RequireAuthorization();

        // --- FORGOT PASSWORD ---
        group.MapPost("/forgot-password", async (string email, IUserRepository repo, IEmailService emailService) =>
        {
            var user = await repo.GetUserByEmailAsync(email);
            if (user == null) return Results.Ok("If an account exists, a code was sent.");

            var resetCode = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(15);

            await repo.UpdateResetCodeAsync(email, resetCode, expiry);

            await emailService.SendEmailAsync(email, "Password Reset", $"Your code is {resetCode}");

            return Results.Ok("Reset code sent.");
        });

        // --- RESET PASSWORD ---
        group.MapPost("/reset-password", async (ResetPasswordRequest request, IUserRepository repo) =>
        {
            var newHashedPassword = PasswordHasher.HashPassword(request.NewPassword);
            var success = await repo.ResetPasswordAsync(request.Email, request.Code, newHashedPassword);

            if (!success) return Results.BadRequest("Invalid code, expired, or incorrect email.");

            return Results.Ok("Password has been reset successfully.");
        });

        group.MapPost("/upload-resume", async (IFormFile file,
            ClaimsPrincipal user,
            [FromServices] IUserRepository repo,
            [FromServices] Supabase.Client supabase) =>
        {
            var userId = repo.GetUserId(user);
            if (userId == Guid.Empty) return Results.Unauthorized();

            if (!user.IsInRole("King"))
            {
                Console.WriteLine("NOT THE KING");
                return Results.Forbid(); // Returns 403 Forbidden
            }

            if (file == null || file.Length == 0) return Results.BadRequest("No file uploaded.");

            // 1. Create a unique filename
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            using var stream = file.OpenReadStream();
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileData = memoryStream.ToArray();

            // 2. Upload to Supabase Storage Bucket 'resumes'
            await supabase.Storage
                    .From("resumes")
                    .Upload(fileData, fileName, new Supabase.Storage.FileOptions { Upsert = true });

            // 3. Get the Public URL
            var publicUrl = supabase.Storage.From("resumes").GetPublicUrl(fileName);

            await repo.UpdateResumeUrlAsync(userId, publicUrl);

            return Results.Ok(new { Url = publicUrl });
        }).RequireAuthorization()
        .DisableAntiforgery(); // Required for minimal API file uploads in some setups

        group.MapPut("/update-profile", async (
            User updatedUser,
            ClaimsPrincipal user,
            IUserRepository repo) =>
        {
            // 1. King Check
            if (!user.IsInRole("King"))
            {
                return Results.Forbid();
            }

            // 2. Identity Check (Always use the Token ID, never trust the Body ID)
            var loggedInUserId = repo.GetUserId(user);
            updatedUser.Id = loggedInUserId;

            // 3. Update
            var success = await repo.UpdateUserProfileAsync(updatedUser);

            return success
                ? Results.Ok(new { Message = "Profile updated successfully, King." })
                : Results.BadRequest("Could not update profile.");
        })
        .RequireAuthorization();

        group.MapGet("/profile/king", async (IUserRepository repo) =>
        {
            var king = await repo.GetUserByRoleAsync("King");

            if (king == null)
            {
                return Results.NotFound(new { Message = "The King profile has not been initialized." });
            }

            return Results.Ok(new
            {
                id = king.Id,
                username = king.Username,
                firstName = king.FirstName,
                lastName = king.LastName,
                title = king.Title,
                bio = king.Bio,
                tagline1 = king.Tagline1,
                tagline2 = king.Tagline2,
                yearsOfExperience = king.YearsOfExperience,
                frontendSkills = king.FrontendSkills,
                backendSkills = king.BackendSkills,
                databaseSkills = king.DatabaseSkills,
                profileImageUrl = king.ProfileImageUrl,
                resumeUrl = king.ResumeUrl,
                email = king.Email
            });
        });
    }

}

// Global record for payloads
public record ResetPasswordRequest(string Email, string Code, string NewPassword);
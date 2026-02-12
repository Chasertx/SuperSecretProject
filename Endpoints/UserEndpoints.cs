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

        // POST /api/users/register - Takes the info from the signup form, checks if it's valid, and creates a new account in the database
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
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Title = request.Title,
                Bio = request.Bio,
                YearsOfExperience = request.YearsOfExperience,
                ProfileImageUrl = request.ProfileImageUrl,
                ResumeUrl = request.ResumeUrl,
                Tagline1 = request.Tagline1,
                Tagline2 = request.Tagline2,
                FrontendSkills = request.FrontendSkills?.ToArray(),
                BackendSkills = request.BackendSkills?.ToArray(),
                DatabaseSkills = request.DatabaseSkills?.ToArray(),
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

        // POST /api/users/login - Checks the email and password; if they match, it gives the user a digital "key card" (token) to access the site
        group.MapPost("/login", async (LoginRequest login, IUserRepository repo, ITokenService tokenService) =>
        {
            var user = await repo.GetUserByEmailAsync(login.Email);

            if (user is null) return Results.Unauthorized();

            bool isPasswordValid = PasswordHasher.VerifyPassword(login.Password, user.PasswordHash);

            if (!isPasswordValid) return Results.Unauthorized();

            var token = tokenService.CreateToken(user);

            Console.WriteLine($"System Access: {user.Username} has logged in.");

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
                    user.Role
                }
            });
        });

        // GET /api/users - Grabs a simplified list of every user so they can be displayed on a public directory or card wall
        group.MapGet("/", async (IUserRepository repo) =>
        {
            var users = await repo.GetAllUsersAsync();

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

        // GET /api/users/{id} - Looks up and shows the full profile details for one specific person using their ID
        group.MapGet("/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            var user = await repo.GetUserByIdAsync(id);
            return user is not null ? Results.Ok(user) : Results.NotFound();
        }).RequireAuthorization();

        // DELETE /api/users/{id} - Permanently deletes a user's account and information from the system
        group.MapDelete("/{id:guid}", async (Guid id, IUserRepository repo) =>
        {
            var user = await repo.GetUserByIdAsync(id);
            if (user is null) return Results.NotFound();

            await repo.DeleteUserAsync(id);
            return Results.NoContent();
        }).RequireAuthorization();

        // POST /api/users/forgot-password - Creates a random 6-digit code and emails it to the user so they can reset their password
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

        // POST /api/users/reset-password - Checks if the 6-digit code is correct and hasn't expired, then saves the new password
        group.MapPost("/reset-password", async (ResetPasswordRequest request, IUserRepository repo) =>
        {
            var newHashedPassword = PasswordHasher.HashPassword(request.NewPassword);
            var success = await repo.ResetPasswordAsync(request.Email, request.Code, newHashedPassword);

            if (!success) return Results.BadRequest("Invalid code, expired, or incorrect email.");

            return Results.Ok("Password has been reset successfully.");
        });

        // POST /api/users/upload-resume - Sends a resume file to cloud storage and saves the web link to the user's profile
        group.MapPost("/upload-resume", async (IFormFile file,
            ClaimsPrincipal user,
            [FromServices] IUserRepository repo,
            [FromServices] Supabase.Client supabase) =>
        {
            var userId = repo.GetUserId(user);
            if (userId == Guid.Empty) return Results.Unauthorized();

            if (!user.IsInRole("King")) return Results.Forbid();

            if (file == null || file.Length == 0) return Results.BadRequest("No file uploaded.");

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            using var stream = file.OpenReadStream();
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            await supabase.Storage.From("resumes").Upload(memoryStream.ToArray(), fileName, new Supabase.Storage.FileOptions { Upsert = true });

            var publicUrl = supabase.Storage.From("resumes").GetPublicUrl(fileName);
            await repo.UpdateResumeUrlAsync(userId, publicUrl);

            return Results.Ok(new { Url = publicUrl });
        }).RequireAuthorization().DisableAntiforgery();

        // PUT /api/users/update-profile - Allows the site owner to change their personal bio, skills, and contact links
        group.MapPut("/update-profile", async (User updatedUser, ClaimsPrincipal user, IUserRepository repo) =>
        {
            if (!user.IsInRole("King")) return Results.Forbid();

            updatedUser.Id = repo.GetUserId(user);
            var success = await repo.UpdateUserProfileAsync(updatedUser);

            return success ? Results.Ok(new { Message = "Profile updated" }) : Results.BadRequest();
        }).RequireAuthorization();

        // GET /api/users/profile/king - Specifically fetches the data for the main portfolio owner to display on the homepage
        group.MapGet("/profile/king", async (IUserRepository repo) =>
        {
            var king = await repo.GetUserByRoleAsync("King");
            if (king == null) return Results.NotFound();

            return Results.Ok(new
            {
                king.Id,
                king.Username,
                king.FirstName,
                king.LastName,
                king.Title,
                king.Bio,
                king.Tagline1,
                king.Tagline2,
                king.YearsOfExperience,
                king.FrontendSkills,
                king.BackendSkills,
                king.DatabaseSkills,
                king.ProfileImageUrl,
                king.ResumeUrl,
                king.Email,
                king.InstagramLink,
                king.LinkedInLink,
                king.GitHubLink,
                king.Role
            });
        });

        // POST /api/users/king/upload-asset - Uploads general files (like profile pictures) to the cloud and links them to the owner
        group.MapPost("/king/upload-asset", async ([FromForm(Name = "file")] IFormFile file, [FromForm(Name = "bucketName")] string bucketName, IUserRepository repo) =>
         {
             if (file == null || string.IsNullOrEmpty(bucketName)) return Results.BadRequest();

             try
             {
                 var url = await repo.GetSupabaseUrlAsync(file, bucketName);
                 var success = await repo.UpdateKingAssetUrlAsync(bucketName, url);

                 return success ? Results.Ok(new { url }) : Results.Problem("DB update failed");
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"File transfer error: {ex.Message}");
                 return Results.Problem(ex.Message);
             }
         }).DisableAntiforgery();
    }
}

public record ResetPasswordRequest(string Email, string Code, string NewPassword);
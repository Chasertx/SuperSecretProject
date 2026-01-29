namespace PortfolioPro.Validators;

using FluentValidation;
using PortfolioPro.Core.DTOs;



public class UserRegisterValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterValidator()
    {
        // --- Core Auth Rules ---
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.");

        // --- Profile Rules ---
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Years of experience cannot be negative.");

        // --- URL Validations (Optional fields) ---
        RuleFor(x => x.ProfileImageUrl)
            .Must(LinkMustBeBeValid).WithMessage("Invalid Profile Image URL.")
            .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl));

        RuleFor(x => x.GitHubLink)
            .Must(LinkMustBeBeValid).WithMessage("Invalid GitHub URL.")
            .When(x => !string.IsNullOrEmpty(x.GitHubLink));

        RuleFor(x => x.LinkedInLink)
            .Must(LinkMustBeBeValid).WithMessage("Invalid LinkedIn URL.")
            .When(x => !string.IsNullOrEmpty(x.LinkedInLink));
    }

    // Helper to check for a valid URL structure
    private bool LinkMustBeBeValid(string? link)
    {
        if (string.IsNullOrWhiteSpace(link)) return true;
        return Uri.TryCreate(link, UriKind.Absolute, out _);
    }
}
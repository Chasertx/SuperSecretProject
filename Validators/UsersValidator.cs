namespace PortfolioPro.Validators;

using FluentValidation;
using PortfolioPro.Core.Models;


/** This is here so you don't touch
my database with any weird stuff. **/

/// <summary>
/// Enforces business rules for User profiles before they hit the database.
/// </summary>
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        // Identity Rules
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PasswordHash).NotEmpty().MinimumLength(8)
            .WithMessage("Your password must be at least 8 characters long.");

        // Name Rules
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);

        // Optional Professional Rules
        RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0);

        // Social Link Validation
        // Note: We use the 'link' parameter in the lambda to pass it to our method
        RuleFor(x => x.GitHubLink)
            .Must(link => BeAValidUrl(link))
            .WithMessage("Invalid GitHub URL format.")
            .When(x => !string.IsNullOrEmpty(x.GitHubLink));

        RuleFor(x => x.LinkedInLink)
            .Must(link => BeAValidUrl(link))
            .WithMessage("Invalid LinkedIn URL format.")
            .When(x => !string.IsNullOrEmpty(x.LinkedInLink));
    }

    // This is the method the compiler was missing
    private bool BeAValidUrl(string? link)
    {
        if (string.IsNullOrWhiteSpace(link)) return true; // Let 'NotEmpty' handle empty checks
        return Uri.TryCreate(link, UriKind.Absolute, out _);
    }
}
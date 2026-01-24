using FluentValidation;
using PortfolioPro.Core.Models;

namespace PortfolioPro.Validators;
/** This is here so you don't touch
my database with any weird stuff. **/

/// <summary>
/// Validation rules for Project entities to ensure data integrity before database insertion.
/// </summary>
public class ProjectValidator : AbstractValidator<Project>
{
    public ProjectValidator()
    {
        // Ensures title isn't empty and isn't too long.
        RuleFor(p => p.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(100).WithMessage("HOW DID YOU MANAGE 100 ON A TITLE?");

        // Mandates a description capped at 500 characters.
        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("Please provide a short description. I don't like reading.")
            .MaximumLength(500).WithMessage("Pretty sure Eminem has songs shorter than this.");

        // Validates that a link is provided and that it actually functions.
        RuleFor(p => p.ProjectUrl)
            .NotEmpty().WithMessage("THE LINK IS THE WHOLE POINT OF THIS COLUMN.")
            .Must(LinkMustBeAUri).WithMessage("Project URL must be a valid web address.");

        // Validates that every project is explicity tied to a user GUID.
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("ARE YOU A PERSON? THIS IS FOR PEOPLE ONLY.");
    }

    /// <summary>
    /// Custom predicate to verify if a string is a properly formatted absolute URI.
    /// </summary>
    private bool LinkMustBeAUri(string? link)
    {
        // Returns false for empty strings; otherwise checks for absolute URL format
        if (string.IsNullOrWhiteSpace(link)) return false;
        return Uri.TryCreate(link, UriKind.Absolute, out _);
    }
}
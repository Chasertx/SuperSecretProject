using FluentValidation;
using PortfolioPro.Core.Models;

namespace PortfolioPro.Validators;
/** This is here so you don't touch
my database with any weird stuff. **/
public class ProjectValidator : AbstractValidator<Project>
{
    public ProjectValidator()
    {

        RuleFor(p => p.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(100).WithMessage("HOW DID YOU MANAGE 100 ON A TITLE?");


        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("Please provide a short description. I don't like reading.")
            .MaximumLength(500).WithMessage("Pretty sure Eminem has songs shorter than this.");

        RuleFor(p => p.ProjectUrl)
            .NotEmpty().WithMessage("THE LINK IS THE WHOLE POINT OF THIS COLUMN.")
            .Must(LinkMustBeAUri).WithMessage("Project URL must be a valid web address.");

        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("ARE YOU A PERSON? THIS IS FOR PEOPLE ONLY.");
    }

    private bool LinkMustBeAUri(string? link)
    {
        if (string.IsNullOrWhiteSpace(link)) return false;
        return Uri.TryCreate(link, UriKind.Absolute, out _);
    }
}
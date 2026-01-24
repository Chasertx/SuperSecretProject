using FluentValidation;
using PortfolioPro.Core.Models;

namespace PortfolioPro.Validators;
/** This is here so you don't touch
my database with any weird stuff. **/

/// <summary>
/// Enforces business rules for User profiles before they hit the database.
/// </summary>
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        // Requires a non-empty username
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Username cannot be empty.");

        // Mandates a first name for the profile.
        RuleFor(u => u.FirstName)
            .NotEmpty().WithMessage("I need a name to let you pass!");

        // Mandates a last name for the profile.
        RuleFor(u => u.LastName)
            .NotEmpty().WithMessage("I don't make the rules, I just think them up and write them down.");

        // Ensures email is present and follows standard email formatting.
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        // Enforces a minimum security threshold for user passwords.
        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}
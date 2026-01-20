using FluentValidation;
using PortfolioPro.Core.Models;

namespace PortfolioPro.Validators;
/** This is here so you don't touch
my database with any weird stuff. **/
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Username cannot be empty.");

        RuleFor(u => u.FirstName)
            .NotEmpty().WithMessage("I need a name to let you pass!");

        RuleFor(u => u.LastName)
            .NotEmpty().WithMessage("I don't make the rules, I just think them up and write them down.");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}
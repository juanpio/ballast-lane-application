using FluentValidation;

namespace BLA.Ordering.Application.Auth.Validators;

public sealed class RegisterUserValidator : AbstractValidator<Commands.RegisterUserCommand>
{
    private const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";

    public RegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(PasswordPattern)
                .WithMessage("Password must contain uppercase, lowercase, a digit, and a special character.");
    }
}

using System.Text.RegularExpressions;
using BLA.Ordering.Domain.Exceptions;

namespace BLA.Ordering.Domain;

public record User
{
    private static readonly Regex PasswordStrengthRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
        RegexOptions.Compiled);

    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string? Name { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    public static User Register(string email, string plainPassword)
    {
        var validatedEmail = ValueObjects.Email.Create(email);

        if (string.IsNullOrWhiteSpace(plainPassword) || !PasswordStrengthRegex.IsMatch(plainPassword))
            throw new InvalidRegistrationException(
                "Password must be at least 8 characters and contain uppercase, lowercase, a digit, and a special character.");

        return new User
        {
            Email = validatedEmail.Value,
            PasswordHash = string.Empty // infrastructure sets this before persistence
        };
    }
}

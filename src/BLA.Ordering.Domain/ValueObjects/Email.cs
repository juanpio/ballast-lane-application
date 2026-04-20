using System.Text.RegularExpressions;
using BLA.Ordering.Domain.Exceptions;

namespace BLA.Ordering.Domain.ValueObjects;

public record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidRegistrationException("Email is required.");

        if (email.Length > 254)
            throw new InvalidRegistrationException("Email must not exceed 254 characters.");

        if (!EmailRegex.IsMatch(email))
            throw new InvalidRegistrationException("Email format is invalid.");

        return new Email(email.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;
}

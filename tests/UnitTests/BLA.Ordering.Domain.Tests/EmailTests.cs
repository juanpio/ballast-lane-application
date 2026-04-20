using BLA.Ordering.Domain.Exceptions;
using BLA.Ordering.Domain.ValueObjects;
using FluentAssertions;

namespace BLA.Ordering.Domain.Tests;

public class EmailTests
{
    // ── Create: happy path ────────────────────────────────────────────────────

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("u.ser+tag@sub.domain.org")]
    public void Create_ValidEmail_ReturnsEmailWithNormalisedValue(string input)
    {
        var email = Email.Create(input);

        email.Value.Should().Be(input.Trim().ToLowerInvariant());
    }

    [Fact]
    public void Create_ValidEmail_ToStringMatchesValue()
    {
        var email = Email.Create("User@Example.com");

        email.ToString().Should().Be(email.Value);
    }

    // ── Create: null / whitespace ─────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Create_NullOrWhitespace_ThrowsInvalidRegistrationException(string? input)
    {
        var act = () => Email.Create(input!);

        act.Should().Throw<InvalidRegistrationException>()
           .WithMessage("*required*");
    }

    // ── Create: length boundary ───────────────────────────────────────────────

    [Fact]
    public void Create_Exactly254Characters_DoesNotThrow()
    {
        // local@domain.tld padded to exactly 254 chars
        var local = new string('a', 244);
        var email = $"{local}@b.com"; // 244 + 1 + 5 = 250? let's be precise
        // Ensure total length is exactly 254
        var padding = new string('a', 254 - "@b.com".Length);
        var validEmail = $"{padding}@b.com";
        validEmail.Length.Should().Be(254);

        var act = () => Email.Create(validEmail);

        act.Should().NotThrow();
    }

    [Fact]
    public void Create_Exceeds254Characters_ThrowsInvalidRegistrationException()
    {
        var local = new string('a', 249);
        var longEmail = $"{local}@b.com"; // 249 + 6 = 255 chars
        longEmail.Length.Should().Be(255);

        var act = () => Email.Create(longEmail);

        act.Should().Throw<InvalidRegistrationException>()
           .WithMessage("*254*");
    }

    // ── Create: invalid format ────────────────────────────────────────────────

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    [InlineData("no spaces@domain.com")]
    [InlineData("double@@domain.com")]
    public void Create_InvalidFormat_ThrowsInvalidRegistrationException(string input)
    {
        var act = () => Email.Create(input);

        act.Should().Throw<InvalidRegistrationException>()
           .WithMessage("*invalid*");
    }

    // ── Record equality ───────────────────────────────────────────────────────

    [Fact]
    public void Create_SameAddressDifferentCase_ProduceEqualEmails()
    {
        var lower = Email.Create("user@example.com");
        var upper = Email.Create("USER@EXAMPLE.COM");

        lower.Should().Be(upper);
    }

    [Fact]
    public void Create_DifferentAddresses_ProduceUnequalEmails()
    {
        var a = Email.Create("alice@example.com");
        var b = Email.Create("bob@example.com");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Create_AddressWithLeadingTrailingSpaces_ThrowsInvalidRegistrationException()
    {
        // Validation runs before trimming, so spaces cause the regex to reject the input.
        var act = () => Email.Create("  user@example.com  ");

        act.Should().Throw<InvalidRegistrationException>()
           .WithMessage("*invalid*");
    }
}

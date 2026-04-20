using BLA.Ordering.Domain;
using BLA.Ordering.Domain.Exceptions;
using FluentAssertions;

namespace BLA.Ordering.Domain.Tests;

public class UserTests
{
    // ── Register: happy path ──────────────────────────────────────────────────

    [Fact]
    public void Register_ValidEmailAndPassword_ReturnsUserWithNormalisedEmail()
    {
        // Arrange
        const string email = "Test@Example.COM";
        const string password = "Str0ng!Pass";

        // Act
        var user = User.Register(email, password);

        // Assert
        user.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void Register_ValidInput_PasswordHashIsEmpty()
    {
        // The hash is set by infrastructure before persistence; domain must not
        // pre-populate it.
        var user = User.Register("user@example.com", "Str0ng!Pass");

        user.PasswordHash.Should().BeEmpty();
    }

    [Fact]
    public void Register_ValidInput_NameIsNull()
    {
        var user = User.Register("user@example.com", "Str0ng!Pass");

        user.Name.Should().BeNull();
    }

    [Theory]
    [InlineData("Passw0rd!")]
    [InlineData("Abcdef1@")]
    [InlineData("C0mpl3x#Password")]
    [InlineData("short1!A")]  // exactly 8 chars with all required character classes
    public void Register_PasswordMeetsAllCriteria_DoesNotThrow(string password)
    {
        var act = () => User.Register("user@example.com", password);

        act.Should().NotThrow();
    }

    // ── Register: invalid email ───────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Register_NullOrWhitespaceEmail_ThrowsInvalidRegistrationException(string? email)
    {
        var act = () => User.Register(email!, "Str0ng!Pass");

        act.Should().Throw<InvalidRegistrationException>()
           .WithMessage("*required*");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    public void Register_MalformedEmail_ThrowsInvalidRegistrationException(string email)
    {
        var act = () => User.Register(email, "Str0ng!Pass");

        act.Should().Throw<InvalidRegistrationException>()
           .WithMessage("*invalid*");
    }

    [Fact]
    public void Register_EmailExceeds254Characters_ThrowsInvalidRegistrationException()
    {
        var longEmail = new string('a', 249) + "@b.com"; // 249 + 6 = 255 chars > 254

        var act = () => User.Register(longEmail, "Str0ng!Pass");

        act.Should().Throw<InvalidRegistrationException>()
           .WithMessage("*254*");
    }

    // ── Register: invalid password ────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Register_NullOrWhitespacePassword_ThrowsInvalidRegistrationException(string? password)
    {
        var act = () => User.Register("user@example.com", password!);

        act.Should().Throw<InvalidRegistrationException>()
           .WithMessage("*Password*");
    }

    [Theory]
    [InlineData("nouppercase1!")]       // no uppercase
    [InlineData("NOLOWERCASE1!")]       // no lowercase
    [InlineData("NoDigitHere!")]        // no digit
    [InlineData("NoSpecial1Char")]      // no special character
    [InlineData("Sh0rt!")]             // fewer than 8 characters
    public void Register_WeakPassword_ThrowsInvalidRegistrationException(string password)
    {
        var act = () => User.Register("user@example.com", password);

        act.Should().Throw<InvalidRegistrationException>()
           .WithMessage("*Password*");
    }

    // ── Record semantics ──────────────────────────────────────────────────────

    [Fact]
    public void Register_TwoCallsWithSameInput_ProduceEqualUsers()
    {
        const string email = "user@example.com";
        const string password = "Str0ng!Pass";

        var first = User.Register(email, password);
        var second = User.Register(email, password);

        first.Should().Be(second);
    }

    [Fact]
    public void Register_DifferentEmails_ProduceUnequalUsers()
    {
        var first = User.Register("alice@example.com", "Str0ng!Pass");
        var second = User.Register("bob@example.com", "Str0ng!Pass");

        first.Should().NotBe(second);
    }
}

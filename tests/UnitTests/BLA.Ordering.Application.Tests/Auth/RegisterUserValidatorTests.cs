using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Application.Auth.Validators;
using FluentAssertions;

namespace BLA.Ordering.Application.Tests.Auth;

public sealed class RegisterUserValidatorTests
{
    private readonly RegisterUserValidator _validator = new();

    [Fact]
    public async Task Validate_ValidEmailAndStrongPassword_ReturnsSuccess()
    {
        // Arrange
        var command = new RegisterUserCommand("user@example.com", "SecurePass1!");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-at.com")]
    [InlineData("")]
    [InlineData("@nodomain.com")]
    public async Task Validate_InvalidEmail_ReturnsEmailError(string invalidEmail)
    {
        // Arrange
        var command = new RegisterUserCommand(invalidEmail, "SecurePass1!");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Theory]
    [InlineData("short1!")]          // less than 8 chars
    [InlineData("alllowercase1!")]   // no uppercase
    [InlineData("ALLUPPERCASE1!")]   // no lowercase
    [InlineData("NoSpecialChar1")]   // no special character
    [InlineData("NoNumber@Pass")]    // no digit
    public async Task Validate_WeakPassword_ReturnsPasswordError(string weakPassword)
    {
        // Arrange
        var command = new RegisterUserCommand("user@example.com", weakPassword);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Fact]
    public async Task Validate_EmptyFields_ReturnsBothErrors()
    {
        // Arrange
        var command = new RegisterUserCommand(string.Empty, string.Empty);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }
}

using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Application.Auth.Validators;
using FluentAssertions;

namespace BLA.Ordering.Application.Tests.Auth;

public sealed class AuthenticateUserValidatorTests
{
    private readonly AuthenticateUserValidator _validator = new();

    [Fact]
    public async Task Validate_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var command = new AuthenticateUserCommand("user@example.com", "SecurePass1!");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("user@")]
    public async Task Validate_InvalidEmail_ReturnsEmailError(string email)
    {
        // Arrange
        var command = new AuthenticateUserCommand(email, "SecurePass1!");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AuthenticateUserCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_EmptyPassword_ReturnsPasswordError(string password)
    {
        // Arrange
        var command = new AuthenticateUserCommand("user@example.com", password);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AuthenticateUserCommand.Password));
    }
}

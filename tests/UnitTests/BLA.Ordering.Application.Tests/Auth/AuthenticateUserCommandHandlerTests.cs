using BLA.Ordering.Application.Auth;
using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Application.Auth.Dtos;
using BLA.Ordering.Domain;
using BLA.Ordering.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BLA.Ordering.Application.Tests.Auth;

public sealed class AuthenticateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly ILogger<AuthenticateUserCommandHandler> _logger =
        Substitute.For<ILogger<AuthenticateUserCommandHandler>>();

    private AuthenticateUserCommandHandler CreateHandler() =>
        new(_userRepository, _passwordHasher, _jwtTokenService, _logger);

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsAuthenticationResult()
    {
        // Arrange
        var command = new AuthenticateUserCommand("user@example.com", "SecurePass1!");
        var user = new User
        {
            Id = 7,
            Email = "user@example.com",
            PasswordHash = "$2b$11$hash",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var token = new TokenResult("jwt-token", expiresAt);

        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("SecurePass1!", "$2b$11$hash").Returns(true);
        _jwtTokenService.GenerateToken(user).Returns(token);

        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.UserId.Should().Be(7);
        result.Email.Should().Be("user@example.com");
        result.AccessToken.Should().Be("jwt-token");
        result.ExpiresAtUtc.Should().Be(expiresAt);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = new AuthenticateUserCommand("missing@example.com", "SecurePass1!");
        _userRepository.GetByEmailAsync("missing@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = CreateHandler();

        // Act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
        _jwtTokenService.DidNotReceiveWithAnyArgs().GenerateToken(default!);
    }

    [Fact]
    public async Task HandleAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = new AuthenticateUserCommand("user@example.com", "WrongPassword9@");
        var user = new User
        {
            Id = 9,
            Email = "user@example.com",
            PasswordHash = "$2b$11$hash",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("WrongPassword9@", "$2b$11$hash").Returns(false);

        var handler = CreateHandler();

        // Act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
        _jwtTokenService.DidNotReceiveWithAnyArgs().GenerateToken(default!);
    }
}

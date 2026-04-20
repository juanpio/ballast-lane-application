using BLA.Ordering.Application.Auth;
using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Domain;
using BLA.Ordering.Domain.Exceptions;
using BLA.Ordering.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BLA.Ordering.Application.Tests.Auth;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ILogger<RegisterUserCommandHandler> _logger =
        Substitute.For<ILogger<RegisterUserCommandHandler>>();

    private RegisterUserCommandHandler CreateHandler() =>
        new(_userRepository, _passwordHasher, _logger);

    [Fact]
    public async Task HandleAsync_ValidInput_ReturnsRegisterUserResult()
    {
        // Arrange
        const string email = "user@example.com";
        const string password = "SecurePass1!";
        const string hash = "$2b$11$hashedpassword";

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.Hash(password).Returns(hash);
        _userRepository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(new User { Id = 1, Email = email, PasswordHash = hash });

        var handler = CreateHandler();
        var command = new RegisterUserCommand(email, password);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.UserId.Should().Be(1);
        result.Email.Should().Be(email);
        await _userRepository.Received(1).CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_EmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        const string email = "existing@example.com";
        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(new User { Id = 42, Email = email, PasswordHash = "$2b$11$hash" });

        var handler = CreateHandler();
        var command = new RegisterUserCommand(email, "SecurePass1!");

        // Act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_InvalidEmail_ThrowsInvalidRegistrationException()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new RegisterUserCommand("not-an-email", "SecurePass1!");

        // Act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidRegistrationException>();
    }

    [Fact]
    public async Task HandleAsync_WeakPassword_ThrowsInvalidRegistrationException()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new RegisterUserCommand("user@example.com", "weak");

        // Act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidRegistrationException>();
    }
}

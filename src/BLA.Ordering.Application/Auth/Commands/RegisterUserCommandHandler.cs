using BLA.Ordering.Application.Auth.Dtos;
using BLA.Ordering.Domain;
using BLA.Ordering.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLA.Ordering.Application.Auth.Commands;

public sealed class RegisterUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<RegisterUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        // Domain validation first (fast, no I/O)
        var user = User.Register(command.Email, command.Password);

        // Check for duplicate email
        var existing = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        // Hash password and attach to user
        var hash = _passwordHasher.Hash(command.Password);
        var userWithHash = user with { PasswordHash = hash };

        var created = await _userRepository.CreateAsync(userWithHash, cancellationToken);

        _logger.LogInformation("User registered successfully. UserId={UserId}", created.Id);

        return new RegisterUserResult(created.Id, created.Email);
    }
}

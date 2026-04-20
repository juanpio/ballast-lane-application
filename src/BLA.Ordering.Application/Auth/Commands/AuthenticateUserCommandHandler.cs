using BLA.Ordering.Application.Auth.Dtos;
using BLA.Ordering.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLA.Ordering.Application.Auth.Commands;

public sealed class AuthenticateUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthenticateUserCommandHandler> _logger;

    public AuthenticateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<AuthenticateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthenticateUserResult> HandleAsync(
        AuthenticateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _jwtTokenService.GenerateToken(user);

        _logger.LogInformation("User authenticated successfully. UserId={UserId}", user.Id);

        return new AuthenticateUserResult(
            user.Id,
            user.Email,
            token.AccessToken,
            token.ExpiresAtUtc);
    }
}

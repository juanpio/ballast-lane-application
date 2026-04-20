namespace BLA.Ordering.Application.Auth.Dtos;

public record AuthenticateUserResult(
    int UserId,
    string Email,
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string TokenType = "Bearer");

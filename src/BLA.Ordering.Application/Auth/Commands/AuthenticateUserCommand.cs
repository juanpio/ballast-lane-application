namespace BLA.Ordering.Application.Auth.Commands;

public record AuthenticateUserCommand(string Email, string Password);

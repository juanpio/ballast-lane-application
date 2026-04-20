namespace BLA.Ordering.Application.Auth.Dtos;

public record RegisterUserRequest(string Email, string Password, string ConfirmPassword);

namespace BLA.Ordering.Application.Auth.Dtos;

public record TokenResult(string AccessToken, DateTimeOffset ExpiresAtUtc);

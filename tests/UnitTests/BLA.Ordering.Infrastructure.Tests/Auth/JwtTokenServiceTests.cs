using System.IdentityModel.Tokens.Jwt;
using BLA.Ordering.Domain;
using BLA.Ordering.Infrastructure.Auth;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace BLA.Ordering.Infrastructure.Tests.Auth;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public void GenerateToken_ValidConfiguration_ReturnsJwtWithExpectedClaims()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "BLA.Ordering",
            ["Jwt:Audience"] = "BLA.Ordering.Client",
            ["Jwt:Key"] = "BLA_LOCAL_DEV_SUPER_SECRET_KEY_CHANGE_ME_1234567890",
            ["Jwt:AccessTokenMinutes"] = "60"
        });

        var service = new JwtTokenService(configuration);
        var user = new User
        {
            Id = 12,
            Email = "user@example.com",
            PasswordHash = "$2b$11$hash",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = service.GenerateToken(user);

        // Assert
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        token.Issuer.Should().Be("BLA.Ordering");
        token.Audiences.Should().ContainSingle().Which.Should().Be("BLA.Ordering.Client");
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "12");
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "user@example.com");
    }

    [Fact]
    public void GenerateToken_MissingIssuer_ThrowsInvalidOperationException()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Audience"] = "BLA.Ordering.Client",
            ["Jwt:Key"] = "BLA_LOCAL_DEV_SUPER_SECRET_KEY_CHANGE_ME_1234567890",
            ["Jwt:AccessTokenMinutes"] = "60"
        });

        var service = new JwtTokenService(configuration);
        var user = new User { Id = 1, Email = "user@example.com", PasswordHash = "hash" };

        // Act
        var act = () => service.GenerateToken(user);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Issuer*");
    }

    [Fact]
    public void GenerateToken_InvalidExpiry_ThrowsInvalidOperationException()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "BLA.Ordering",
            ["Jwt:Audience"] = "BLA.Ordering.Client",
            ["Jwt:Key"] = "BLA_LOCAL_DEV_SUPER_SECRET_KEY_CHANGE_ME_1234567890",
            ["Jwt:AccessTokenMinutes"] = "abc"
        });

        var service = new JwtTokenService(configuration);
        var user = new User { Id = 1, Email = "user@example.com", PasswordHash = "hash" };

        // Act
        var act = () => service.GenerateToken(user);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AccessTokenMinutes*");
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
}
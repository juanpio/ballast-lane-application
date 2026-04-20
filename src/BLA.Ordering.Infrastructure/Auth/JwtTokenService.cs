using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BLA.Ordering.Application.Auth;
using BLA.Ordering.Application.Auth.Dtos;
using BLA.Ordering.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLA.Ordering.Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration) => _configuration = configuration;

    public TokenResult GenerateToken(User user)
    {
        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var expiryMinutesText = _configuration["Jwt:AccessTokenMinutes"] ?? "60";

        if (!int.TryParse(expiryMinutesText, out var expiryMinutes) || expiryMinutes <= 0)
            throw new InvalidOperationException("Jwt:AccessTokenMinutes must be a positive integer.");

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var serializedToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        return new TokenResult(serializedToken, expiresAt);
    }
}

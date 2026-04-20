using BLA.Ordering.Application.Auth.Dtos;
using BLA.Ordering.Domain;

namespace BLA.Ordering.Application.Auth;

public interface IJwtTokenService
{
    TokenResult GenerateToken(User user);
}

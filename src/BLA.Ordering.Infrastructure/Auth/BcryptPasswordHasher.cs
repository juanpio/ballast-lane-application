using BLA.Ordering.Application.Auth;

namespace BLA.Ordering.Infrastructure.Auth;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => throw new NotImplementedException();

    public bool Verify(string plainPassword, string hash) =>  throw new NotImplementedException();
}

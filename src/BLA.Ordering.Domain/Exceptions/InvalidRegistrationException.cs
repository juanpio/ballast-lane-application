namespace BLA.Ordering.Domain.Exceptions;

public sealed class InvalidRegistrationException : DomainException
{
    public InvalidRegistrationException(string message) : base(message) { }
}

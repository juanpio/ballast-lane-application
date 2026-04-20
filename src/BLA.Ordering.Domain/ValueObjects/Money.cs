namespace BLA.Ordering.Domain.ValueObjects;
public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } // "USD" or "EUR"

    // Private constructor ensures creation only via factory methods
    private Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        
        Amount = amount;
        Currency = currency;
    }

    // Factory methods for valid currencies
    public static Money USD(decimal amount) => new(amount, "USD");
    public static Money EUR(decimal amount) => new(amount, "EUR");

    // Example of domain logic: Adding money
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");

        return new Money(Amount + other.Amount, Currency);
    }
}
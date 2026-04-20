namespace BLA.Ordering.Domain;

public class Order
{
    public required string Id { get; init; }
    public required string CustomerId { get; init; }
    public required DateTime OrderDate { get; init; }
    public required string ShippingAddress { get; init; }
    public required Money TotalAmount { get; init; }

}

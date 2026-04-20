namespace BLA.Ordering.Application.Orders.Commands;

public record UpdateOrderCommand(
    string Id,
    string CustomerId,
    string? ShippingAddress,
    decimal? TotalAmount,
    string? Currency
);

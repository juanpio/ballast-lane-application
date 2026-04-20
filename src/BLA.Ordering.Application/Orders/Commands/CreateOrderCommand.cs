namespace BLA.Ordering.Application.Orders.Commands;

public record CreateOrderCommand(
    string Id,
    string ShippingAddress,
    decimal TotalAmount,
    string Currency,
    string CustomerId
);

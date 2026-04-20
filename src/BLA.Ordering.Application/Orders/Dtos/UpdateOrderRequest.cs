namespace BLA.Ordering.Application.Orders.Dtos;

public record UpdateOrderRequest(
    string? ShippingAddress,
    decimal? TotalAmount,
    string? Currency
);

namespace BLA.Ordering.Application.Orders.Dtos;

public record CreateOrderRequest(
    string Id,
    string ShippingAddress,
    decimal TotalAmount,
    string Currency
);

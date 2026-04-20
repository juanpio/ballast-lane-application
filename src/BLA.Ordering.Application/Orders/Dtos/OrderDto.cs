namespace BLA.Ordering.Application.Orders.Dtos;

public record OrderDto(
    string Id,
    string CustomerId,
    DateTime OrderDate,
    string ShippingAddress,
    decimal TotalAmount,
    string Currency
);

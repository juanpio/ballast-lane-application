namespace BLA.Ordering.Infrastructure.Models;

public static class OrderMappingModel
{
    public record OrderModel(
        string Id,
        string CustomerId,
        DateTime OrderDate,
        string ShippingAddress,
        decimal TotalAmount,
        string Currency = "USD"
    );

    public static OrderModel FromDomain(Domain.Order order)
    {
        return new OrderModel(
            order.Id,
            order.CustomerId,
            order.OrderDate,
            order.ShippingAddress,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency
        );
    }

    public static Domain.Order ToDomain(OrderModel model)
    {
        var money = model.Currency switch
        {
            "EUR" => Domain.ValueObjects.Money.EUR(model.TotalAmount),
            _ => Domain.ValueObjects.Money.USD(model.TotalAmount)
        };

        return new Domain.Order
        {
            Id = model.Id,
            CustomerId = model.CustomerId,
            OrderDate = model.OrderDate,
            ShippingAddress = model.ShippingAddress,
            TotalAmount = money
        };
    }
}

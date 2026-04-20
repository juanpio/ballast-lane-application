using BLA.Ordering.Application.Orders.Dtos;
using BLA.Ordering.Domain;
using BLA.Ordering.Domain.Interfaces;
using BLA.Ordering.Domain.ValueObjects;

namespace BLA.Ordering.Application.Orders.Commands;

public sealed class CreateOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedCurrency = command.Currency.Trim().ToUpperInvariant();
        var money = normalizedCurrency switch
        {
            "EUR" => Money.EUR(command.TotalAmount),
            _ => Money.USD(command.TotalAmount)
        };

        var order = new Order
        {
            Id = command.Id.Trim(),
            CustomerId = command.CustomerId,
            OrderDate = DateTime.UtcNow,
            ShippingAddress = command.ShippingAddress.Trim(),
            TotalAmount = money
        };

        var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);

        return new OrderDto(
            createdOrder.Id,
            createdOrder.CustomerId,
            createdOrder.OrderDate,
            createdOrder.ShippingAddress,
            createdOrder.TotalAmount.Amount,
            createdOrder.TotalAmount.Currency);
    }
}

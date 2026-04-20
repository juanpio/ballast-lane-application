using BLA.Ordering.Application.Orders.Dtos;
using BLA.Ordering.Domain;
using BLA.Ordering.Domain.Interfaces;
using BLA.Ordering.Domain.ValueObjects;

namespace BLA.Ordering.Application.Orders.Commands;

public sealed class UpdateOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> HandleAsync(UpdateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var existingOrder = await _orderRepository.GetByIdForCustomerAsync(command.Id, command.CustomerId, cancellationToken);
        if (existingOrder is null)
            throw new KeyNotFoundException($"Order '{command.Id}' was not found for this user.");

        var currency = command.Currency?.Trim().ToUpperInvariant() ?? existingOrder.TotalAmount.Currency;
        var amount = command.TotalAmount ?? existingOrder.TotalAmount.Amount;

        var money = currency switch
        {
            "EUR" => Money.EUR(amount),
            _ => Money.USD(amount)
        };

        var updatedOrder = new Order
        {
            Id = existingOrder.Id,
            CustomerId = existingOrder.CustomerId,
            OrderDate = existingOrder.OrderDate,
            ShippingAddress = command.ShippingAddress?.Trim() ?? existingOrder.ShippingAddress,
            TotalAmount = money
        };

        var savedOrder = await _orderRepository.UpdateAsync(updatedOrder, cancellationToken);
        if (savedOrder is null)
            throw new KeyNotFoundException($"Order '{command.Id}' was not found for this user.");

        return new OrderDto(
            savedOrder.Id,
            savedOrder.CustomerId,
            savedOrder.OrderDate,
            savedOrder.ShippingAddress,
            savedOrder.TotalAmount.Amount,
            savedOrder.TotalAmount.Currency);
    }
}

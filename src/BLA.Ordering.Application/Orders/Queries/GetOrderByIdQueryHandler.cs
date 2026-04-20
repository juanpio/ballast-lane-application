using BLA.Ordering.Application.Orders.Dtos;
using BLA.Ordering.Domain.Interfaces;

namespace BLA.Ordering.Application.Orders.Queries;

public sealed class GetOrderByIdQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> HandleAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdForCustomerAsync(query.Id, query.CustomerId, cancellationToken);
        if (order is null)
            return null;

        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.OrderDate,
            order.ShippingAddress,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency);
    }
}

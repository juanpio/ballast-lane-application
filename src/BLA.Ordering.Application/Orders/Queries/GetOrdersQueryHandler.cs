using BLA.Ordering.Application.Orders.Dtos;
using BLA.Ordering.Domain.Interfaces;

namespace BLA.Ordering.Application.Orders.Queries;

public sealed class GetOrdersQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedOrdersResponse> HandleAsync(GetOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var (orders, total) = await _orderRepository.GetPagedForCustomerAsync(
            query.CustomerId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var mappedOrders = orders
            .Select(order => new OrderDto(
                order.Id,
                order.CustomerId,
                order.OrderDate,
                order.ShippingAddress,
                order.TotalAmount.Amount,
                order.TotalAmount.Currency))
            .ToList();

        return new PagedOrdersResponse(mappedOrders, total, query.Page, query.PageSize);
    }
}

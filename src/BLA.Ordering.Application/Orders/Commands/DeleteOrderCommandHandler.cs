using BLA.Ordering.Domain.Interfaces;

namespace BLA.Ordering.Application.Orders.Commands;

public sealed class DeleteOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Task<bool> HandleAsync(DeleteOrderCommand command, CancellationToken cancellationToken = default)
    {
        return _orderRepository.SoftDeleteAsync(command.Id, command.CustomerId, cancellationToken);
    }
}

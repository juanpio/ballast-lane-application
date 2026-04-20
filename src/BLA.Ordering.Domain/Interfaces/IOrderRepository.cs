namespace BLA.Ordering.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdForCustomerAsync(string id, string customerId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Order> Orders, int Total)> GetPagedForCustomerAsync(
        string customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(string id, string customerId, CancellationToken cancellationToken = default);
}

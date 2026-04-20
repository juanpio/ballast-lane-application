using BLA.Ordering.Domain;

namespace BLA.Ordering.Infrastructure;

public interface IUnitOfWork
{
    Task CommitAsync();
}

public class UnitOfWork : IUnitOfWork
{
    public Task CommitAsync()
    {
        // In a real implementation, this would commit transactions to the database
        return Task.CompletedTask;
    }
}

public interface IRepository<T>
{
    Task<T> GetByIdAsync(string id);
    Task SaveAsync(T entity);
    Task DeleteAsync(string id);
}

public interface IOrderRepository : IRepository<Order>;

public class OrderRepository : IOrderRepository
{
    public Task<Order> GetByIdAsync(string id)
    {
        // TODO: implement with parameterized NpgsqlCommand (ADR 005)
        return Task.FromResult(new Order
        {
            Id = id,
            CustomerId = string.Empty,
            OrderDate = DateTime.UtcNow,
            ShippingAddress = string.Empty,
            TotalAmount = Domain.ValueObjects.Money.USD(0)
        });
    }

    public Task SaveAsync(Order entity)
    {
        // In a real implementation, this would save the order to the database
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        // In a real implementation, this would delete the order from the database
        return Task.CompletedTask;
    }

}

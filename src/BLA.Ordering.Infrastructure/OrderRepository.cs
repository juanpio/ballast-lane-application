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
        // In a real implementation, this would retrieve the order from the database
        return Task.FromResult(new Order { Id = id });
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

using BLA.Ordering.Domain;
using BLA.Ordering.Domain.Interfaces;
using BLA.Ordering.Domain.ValueObjects;
using Npgsql;

namespace BLA.Ordering.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async Task<Order?> GetByIdForCustomerAsync(string id, string customerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateOrderId(id);
        var parsedCustomerId = ParseCustomerId(customerId);

        const string sql = """
            SELECT id, customer_id, order_date, shipping_address, total_amount, currency
            FROM domain.orders
            WHERE id = @id
              AND customer_id = @customerId
              AND deleted_at IS NULL
            LIMIT 1
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id.Trim());
        command.Parameters.AddWithValue("@customerId", parsedCustomerId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapOrder(reader);
    }

    public async Task<(IReadOnlyList<Order> Orders, int Total)> GetPagedForCustomerAsync(
        string customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var parsedCustomerId = ParseCustomerId(customerId);

        if (page <= 0)
            throw new ArgumentException("Page must be greater than zero.", nameof(page));

        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

        var offset = (page - 1) * pageSize;

        const string listSql = """
            SELECT id, customer_id, order_date, shipping_address, total_amount, currency
            FROM domain.orders
            WHERE customer_id = @customerId
              AND deleted_at IS NULL
            ORDER BY updated_at DESC
            OFFSET @offset
            LIMIT @pageSize
            """;

        const string countSql = """
            SELECT COUNT(1)
            FROM domain.orders
            WHERE customer_id = @customerId
              AND deleted_at IS NULL
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var orders = new List<Order>(pageSize);

        await using (var listCommand = new NpgsqlCommand(listSql, connection))
        {
            listCommand.Parameters.AddWithValue("@customerId", parsedCustomerId);
            listCommand.Parameters.AddWithValue("@offset", offset);
            listCommand.Parameters.AddWithValue("@pageSize", pageSize);

            await using var reader = await listCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                orders.Add(MapOrder(reader));
        }

        await using var countCommand = new NpgsqlCommand(countSql, connection);
        countCommand.Parameters.AddWithValue("@customerId", parsedCustomerId);

        var total = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));
        return (orders, total);
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateOrder(order);

        const string sql = """
            INSERT INTO domain.orders (
                id,
                customer_id,
                order_date,
                shipping_address,
                total_amount,
                currency,
                created_at,
                updated_at)
            VALUES (
                @id,
                @customerId,
                @orderDate,
                @shippingAddress,
                @totalAmount,
                @currency,
                now(),
                now())
            RETURNING id, customer_id, order_date, shipping_address, total_amount, currency
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", order.Id.Trim());
        command.Parameters.AddWithValue("@customerId", ParseCustomerId(order.CustomerId));
        command.Parameters.AddWithValue("@orderDate", order.OrderDate);
        command.Parameters.AddWithValue("@shippingAddress", order.ShippingAddress.Trim());
        command.Parameters.AddWithValue("@totalAmount", order.TotalAmount.Amount);
        command.Parameters.AddWithValue("@currency", order.TotalAmount.Currency);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return MapOrder(reader);
    }

    public async Task<Order?> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateOrder(order);

        const string sql = """
            UPDATE domain.orders
            SET shipping_address = @shippingAddress,
                total_amount = @totalAmount,
                currency = @currency,
                updated_at = now()
            WHERE id = @id
              AND customer_id = @customerId
              AND deleted_at IS NULL
            RETURNING id, customer_id, order_date, shipping_address, total_amount, currency
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", order.Id.Trim());
        command.Parameters.AddWithValue("@customerId", ParseCustomerId(order.CustomerId));
        command.Parameters.AddWithValue("@shippingAddress", order.ShippingAddress.Trim());
        command.Parameters.AddWithValue("@totalAmount", order.TotalAmount.Amount);
        command.Parameters.AddWithValue("@currency", order.TotalAmount.Currency);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapOrder(reader);
    }

    public async Task<bool> SoftDeleteAsync(string id, string customerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateOrderId(id);
        var parsedCustomerId = ParseCustomerId(customerId);

        const string sql = """
            UPDATE domain.orders
            SET deleted_at = now(),
                updated_at = now()
            WHERE id = @id
              AND customer_id = @customerId
              AND deleted_at IS NULL
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id.Trim());
        command.Parameters.AddWithValue("@customerId", parsedCustomerId);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    private static int ParseCustomerId(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        if (!int.TryParse(customerId, out var parsedCustomerId) || parsedCustomerId <= 0)
            throw new ArgumentException("Customer id must be a positive integer.", nameof(customerId));

        return parsedCustomerId;
    }

    private static void ValidateOrder(Order order)
    {
        if (order is null)
            throw new ArgumentNullException(nameof(order));

        ValidateOrderId(order.Id);

        if (string.IsNullOrWhiteSpace(order.CustomerId))
            throw new ArgumentException("Order customer id is required.", nameof(order));

        if (string.IsNullOrWhiteSpace(order.ShippingAddress))
            throw new ArgumentException("Order shipping address is required.", nameof(order));

        if (order.TotalAmount.Amount <= 0)
            throw new ArgumentException("Order total amount must be greater than zero.", nameof(order));
    }

    private static void ValidateOrderId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Order id is required.", nameof(id));
    }

    private static Order MapOrder(NpgsqlDataReader reader)
    {
        var currency = reader.GetString(5);
        var amount = reader.GetDecimal(4);

        var money = currency switch
        {
            "EUR" => Money.EUR(amount),
            _ => Money.USD(amount)
        };

        return new Order
        {
            Id = reader.GetString(0),
            CustomerId = reader.GetInt32(1).ToString(),
            OrderDate = reader.GetFieldValue<DateTime>(2),
            ShippingAddress = reader.GetString(3),
            TotalAmount = money
        };
    }
}

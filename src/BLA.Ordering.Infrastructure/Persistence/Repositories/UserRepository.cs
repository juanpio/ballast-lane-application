using BLA.Ordering.Domain;
using BLA.Ordering.Domain.Interfaces;
using Npgsql;

namespace BLA.Ordering.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UserRepository(NpgsqlDataSource dataSource) =>
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        const string sql = """
            SELECT id, email, password_hash, created_at, updated_at
            FROM auth.users
            WHERE email = @email
            LIMIT 1
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", email.Trim().ToLowerInvariant());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapUser(reader);
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user is null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("User email is required.", nameof(user));

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new ArgumentException("User password hash is required.", nameof(user));

        const string sql = """
            INSERT INTO auth.users (email, password_hash, created_at, updated_at)
            VALUES (@email, @passwordHash, now(), now())
            RETURNING id, email, password_hash, created_at, updated_at
            """;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return MapUser(reader);
    }

    private static User MapUser(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Email = reader.GetString(1),
        PasswordHash = reader.GetString(2),
        CreatedAt = reader.GetFieldValue<DateTimeOffset>(3),
        UpdatedAt = reader.GetFieldValue<DateTimeOffset>(4)
    };
}

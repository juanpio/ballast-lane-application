using BLA.Ordering.Domain;
using BLA.Ordering.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Npgsql;

namespace BLA.Ordering.Infrastructure.Tests.Persistence;

public sealed class UserRepositoryTests
{
    [Fact]
    public void Constructor_NullDataSource_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new UserRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("dataSource");
    }

    [Fact]
    public async Task GetByEmailAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        await using var dataSource = CreateDataSource();
        var repository = new UserRepository(dataSource);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = () => repository.GetByEmailAsync("user@example.com", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetByEmailAsync_InvalidEmail_ThrowsArgumentException(string invalidEmail)
    {
        // Arrange
        await using var dataSource = CreateDataSource();
        var repository = new UserRepository(dataSource);

        // Act
        var act = () => repository.GetByEmailAsync(invalidEmail, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("email");
    }

    [Fact]
    public async Task CreateAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        await using var dataSource = CreateDataSource();
        var repository = new UserRepository(dataSource);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var user = new User
        {
            Email = "user@example.com",
            PasswordHash = "$2b$11$hash"
        };

        // Act
        var act = () => repository.CreateAsync(user, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CreateAsync_NullUser_ThrowsArgumentNullException()
    {
        // Arrange
        await using var dataSource = CreateDataSource();
        var repository = new UserRepository(dataSource);

        // Act
        var act = () => repository.CreateAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("user");
    }

    [Theory]
    [InlineData("", "$2b$11$hash")]
    [InlineData(" ", "$2b$11$hash")]
    public async Task CreateAsync_InvalidUserEmail_ThrowsArgumentException(string email, string hash)
    {
        // Arrange
        await using var dataSource = CreateDataSource();
        var repository = new UserRepository(dataSource);

        var user = new User
        {
            Email = email,
            PasswordHash = hash
        };

        // Act
        var act = () => repository.CreateAsync(user, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("user");
    }

    [Theory]
    [InlineData("user@example.com", "")]
    [InlineData("user@example.com", " ")]
    public async Task CreateAsync_InvalidPasswordHash_ThrowsArgumentException(string email, string hash)
    {
        // Arrange
        await using var dataSource = CreateDataSource();
        var repository = new UserRepository(dataSource);

        var user = new User
        {
            Email = email,
            PasswordHash = hash
        };

        // Act
        var act = () => repository.CreateAsync(user, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("user");
    }

    private static NpgsqlDataSource CreateDataSource() =>
        NpgsqlDataSource.Create("Host=localhost;Port=5432;Database=placeholder;Username=placeholder;Password=placeholder");
}
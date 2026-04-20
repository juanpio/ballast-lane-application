using BLA.Ordering.Application.Orders.Commands;
using BLA.Ordering.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace BLA.Ordering.Application.Tests.Orders;

public sealed class DeleteOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();

    [Fact]
    public async Task HandleAsync_WhenRepositoryDeletesOrder_ReturnsTrue()
    {
        // Arrange
        var command = new DeleteOrderCommand("ORD-3", "4");
        _orderRepository.SoftDeleteAsync(command.Id, command.CustomerId, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new DeleteOrderCommandHandler(_orderRepository);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }
}

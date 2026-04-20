using BLA.Ordering.Application.Orders.Commands;
using BLA.Ordering.Domain;
using BLA.Ordering.Domain.Interfaces;
using BLA.Ordering.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BLA.Ordering.Application.Tests.Orders;

public sealed class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsCreatedOrderDto()
    {
        // Arrange
        var command = new CreateOrderCommand("ORD-1", "123 Main St", 45.50m, "USD", "7");
        var createdOrder = new Order
        {
            Id = "ORD-1",
            CustomerId = "7",
            OrderDate = DateTime.UtcNow,
            ShippingAddress = "123 Main St",
            TotalAmount = Money.USD(45.50m)
        };

        _orderRepository.CreateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(createdOrder);

        var handler = new CreateOrderCommandHandler(_orderRepository);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be("ORD-1");
        result.CustomerId.Should().Be("7");
        result.Currency.Should().Be("USD");
        result.TotalAmount.Should().Be(45.50m);
    }
}

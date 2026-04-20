using BLA.Ordering.Application.Orders.Commands;
using BLA.Ordering.Domain;
using BLA.Ordering.Domain.Interfaces;
using BLA.Ordering.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BLA.Ordering.Application.Tests.Orders;

public sealed class UpdateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();

    [Fact]
    public async Task HandleAsync_OrderNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = new UpdateOrderCommand("ORD-404", "3", "New Street", null, null);
        _orderRepository.GetByIdForCustomerAsync(command.Id, command.CustomerId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var handler = new UpdateOrderCommandHandler(_orderRepository);

        // Act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ExistingOrder_UpdatesRequestedFields()
    {
        // Arrange
        var existingOrder = new Order
        {
            Id = "ORD-2",
            CustomerId = "9",
            OrderDate = DateTime.UtcNow.AddDays(-1),
            ShippingAddress = "Old St",
            TotalAmount = Money.USD(50m)
        };

        var command = new UpdateOrderCommand("ORD-2", "9", "New St", 99.95m, "EUR");

        _orderRepository.GetByIdForCustomerAsync(command.Id, command.CustomerId, Arg.Any<CancellationToken>())
            .Returns(existingOrder);

        _orderRepository.UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Order>());

        var handler = new UpdateOrderCommandHandler(_orderRepository);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be("ORD-2");
        result.ShippingAddress.Should().Be("New St");
        result.TotalAmount.Should().Be(99.95m);
        result.Currency.Should().Be("EUR");
    }
}

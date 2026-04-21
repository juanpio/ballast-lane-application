using System.Security.Claims;
using BLA.Ordering.Application.Orders.Commands;
using BLA.Ordering.Application.Orders.Dtos;
using BLA.Ordering.Application.Orders.Queries;
using BLA.Ordering.Application.Orders.Validators;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLA.Ordering.Web.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/orders")]
[Authorize(Policy = "ApiJwtPolicy")]
public sealed class OrdersController : ControllerBase
{
    private readonly CreateOrderCommandHandler _createOrderCommandHandler;
    private readonly UpdateOrderCommandHandler _updateOrderCommandHandler;
    private readonly DeleteOrderCommandHandler _deleteOrderCommandHandler;
    private readonly GetOrderByIdQueryHandler _getOrderByIdQueryHandler;
    private readonly GetOrdersQueryHandler _getOrdersQueryHandler;
    private readonly CreateOrderValidator _createOrderValidator;
    private readonly UpdateOrderValidator _updateOrderValidator;
    private readonly DeleteOrderValidator _deleteOrderValidator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        CreateOrderCommandHandler createOrderCommandHandler,
        UpdateOrderCommandHandler updateOrderCommandHandler,
        DeleteOrderCommandHandler deleteOrderCommandHandler,
        GetOrderByIdQueryHandler getOrderByIdQueryHandler,
        GetOrdersQueryHandler getOrdersQueryHandler,
        CreateOrderValidator createOrderValidator,
        UpdateOrderValidator updateOrderValidator,
        DeleteOrderValidator deleteOrderValidator,
        ILogger<OrdersController> logger)
    {
        _createOrderCommandHandler = createOrderCommandHandler;
        _updateOrderCommandHandler = updateOrderCommandHandler;
        _deleteOrderCommandHandler = deleteOrderCommandHandler;
        _getOrderByIdQueryHandler = getOrderByIdQueryHandler;
        _getOrdersQueryHandler = getOrdersQueryHandler;
        _createOrderValidator = createOrderValidator;
        _updateOrderValidator = updateOrderValidator;
        _deleteOrderValidator = deleteOrderValidator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedOrdersResponse>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var customerId = GetAuthenticatedCustomerId();
        if (customerId is null)
            return Unauthorized(new { message = "Invalid access token." });

        if (page <= 0)
            return BadRequest(new { message = "Page must be greater than zero." });

        if (pageSize <= 0 || pageSize > 100)
            return BadRequest(new { message = "Page size must be between 1 and 100." });

        var query = new GetOrdersQuery(customerId, page, pageSize);
        var response = await _getOrdersQueryHandler.HandleAsync(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var customerId = GetAuthenticatedCustomerId();
        if (customerId is null)
            return Unauthorized(new { message = "Invalid access token." });

        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { message = "Order id is required." });

        var query = new GetOrderByIdQuery(id, customerId);
        var order = await _getOrderByIdQueryHandler.HandleAsync(query, cancellationToken);
        if (order is null)
            return NotFound(new { message = $"Order '{id}' was not found for this user." });

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = GetAuthenticatedCustomerId();
        if (customerId is null)
            return Unauthorized(new { message = "Invalid access token." });

        var command = new CreateOrderCommand(
            request.Id,
            request.ShippingAddress,
            request.TotalAmount,
            request.Currency,
            customerId);

        var validationResult = await _createOrderValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return ValidationProblem(ModelState);
        }

        try
        {
            var createdOrder = await _createOrderCommandHandler.HandleAsync(command, cancellationToken);
            return Created($"/api/orders/{createdOrder.Id}", createdOrder);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create order for customer {CustomerId}", customerId);
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<OrderDto>> UpdateOrder(
        [FromRoute] string id,
        [FromBody] UpdateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = GetAuthenticatedCustomerId();
        if (customerId is null)
            return Unauthorized(new { message = "Invalid access token." });

        var command = new UpdateOrderCommand(
            id,
            customerId,
            request.ShippingAddress,
            request.TotalAmount,
            request.Currency);

        var validationResult = await _updateOrderValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return ValidationProblem(ModelState);
        }

        try
        {
            var updatedOrder = await _updateOrderCommandHandler.HandleAsync(command, cancellationToken);
            return Ok(updatedOrder);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order {OrderId} not found for customer {CustomerId}", id, customerId);
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var customerId = GetAuthenticatedCustomerId();
        if (customerId is null)
            return Unauthorized(new { message = "Invalid access token." });

        var command = new DeleteOrderCommand(id, customerId);

        var validationResult = await _deleteOrderValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return ValidationProblem(ModelState);
        }

        var deleted = await _deleteOrderCommandHandler.HandleAsync(command, cancellationToken);
        if (!deleted)
            return NotFound(new { message = $"Order '{id}' was not found for this user." });

        return NoContent();
    }

    private string? GetAuthenticatedCustomerId()
    {
        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(nameIdentifier))
            return nameIdentifier;

        return User.FindFirstValue("sub");
    }
}

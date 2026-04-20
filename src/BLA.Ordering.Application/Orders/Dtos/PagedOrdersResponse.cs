namespace BLA.Ordering.Application.Orders.Dtos;

public record PagedOrdersResponse(
    IReadOnlyList<OrderDto> Data,
    int Total,
    int Page,
    int PageSize
);

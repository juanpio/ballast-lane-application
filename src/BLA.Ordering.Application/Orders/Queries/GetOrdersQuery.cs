namespace BLA.Ordering.Application.Orders.Queries;

public record GetOrdersQuery(string CustomerId, int Page = 1, int PageSize = 20);

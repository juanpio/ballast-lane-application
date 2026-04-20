using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BLA.Ordering.Application.Orders.Dtos;
using BLA.Ordering.Web.API.Tests.TestInfrastructure;
using FluentAssertions;

namespace BLA.Ordering.Web.API.Tests.Orders;

public sealed class OrdersApiControllerTests : IClassFixture<OrdersApiTestFixture>
{
    private readonly OrdersApiTestFixture _fixture;

    public OrdersApiControllerTests(OrdersApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetOrders_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        using var client = _fixture.Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/orders?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOrders_WithValidToken_ReturnsOnlyLoggedInUserOrders()
    {
        // Arrange
        using var client = _fixture.Factory.CreateClient();
        var accessToken = await AuthenticateAndGetTokenAsync(client, "user1@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await client.GetAsync("/api/orders?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<PagedOrdersResponse>();
        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeEmpty();
        payload.Data.Should().OnlyContain(order => order.CustomerId == "1001");
        payload.Data.Select(order => order.Id).Should().Contain("ORD-U1-1");
        payload.Data.Select(order => order.Id).Should().Contain("ORD-U1-2");
        payload.Data.Select(order => order.Id).Should().NotContain("ORD-U2-1");
        payload.Data.Select(order => order.Id).Should().NotContain("ORD-U1-DELETED");
    }

    [Fact]
    public async Task GetOrderById_WhenOrderBelongsToAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var client = _fixture.Factory.CreateClient();
        var accessToken = await AuthenticateAndGetTokenAsync(client, "user1@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await client.GetAsync("/api/orders/ORD-U2-1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrderById_WhenOrderBelongsToLoggedInUser_ReturnsOrder()
    {
        // Arrange
        using var client = _fixture.Factory.CreateClient();
        var accessToken = await AuthenticateAndGetTokenAsync(client, "user1@example.com", "Password1!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await client.GetAsync("/api/orders/ORD-U1-1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<OrderDto>();
        payload.Should().NotBeNull();
        payload!.Id.Should().Be("ORD-U1-1");
        payload.CustomerId.Should().Be("1001");
    }

    private static async Task<string> AuthenticateAndGetTokenAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);

        document.RootElement.TryGetProperty("accessToken", out var tokenProperty).Should().BeTrue();
        var accessToken = tokenProperty.GetString();
        accessToken.Should().NotBeNullOrWhiteSpace();

        return accessToken!;
    }
}

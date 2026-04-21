using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Npgsql;
using Reqnroll;

namespace BLA.Ordering.Web.API.Tests.Steps;

[Binding]
public sealed class ApiMvcIntegrationSteps : IAsyncDisposable
{
    private const string BaseUrlEnvName = "BLA_INTEGRATION_BASE_URL";
    private const string DbConnectionEnvName = "BLA_INTEGRATION_DB_CONNECTION";
    private const string DefaultBaseUrl = "http://localhost:5178";
    private const string DefaultDbConnection = "Host=localhost;Port=5432;Database=bla_ordering;Username=postgres;Password=postgres";

    private readonly HttpClient _httpClient;
    private readonly NpgsqlDataSource _dataSource;
    private readonly Dictionary<string, TestCustomer> _customers = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _managedOrderIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _scenarioSuffix = Guid.NewGuid().ToString("N")[..8];

    private HttpResponseMessage? _lastResponse;
    private JsonDocument? _lastJson;

    public ApiMvcIntegrationSteps()
    {
        var baseUrl = Environment.GetEnvironmentVariable(BaseUrlEnvName);
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = DefaultBaseUrl;

        var dbConnection = Environment.GetEnvironmentVariable(DbConnectionEnvName);
        if (string.IsNullOrWhiteSpace(dbConnection))
            dbConnection = DefaultDbConnection;

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute)
        };

        _dataSource = NpgsqlDataSource.Create(dbConnection);
    }

    [Given("the dockerized application is running")]
    public async Task GivenTheDockerizedApplicationIsRunningAsync()
    {
        using var response = await _httpClient.GetAsync("/account/login");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Given("a seeded customer {string} with password {string}")]
    public async Task GivenASeededCustomerWithPasswordAsync(string alias, string password)
    {
        var normalizedAlias = NormalizeAlias(alias);
        var email = $"it-{normalizedAlias}-{_scenarioSuffix}@example.com";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        const string upsertUserSql = """
            INSERT INTO auth.users (email, password_hash, created_at, updated_at)
            VALUES (@email, @passwordHash, now(), now())
            ON CONFLICT (email) DO UPDATE
              SET password_hash = EXCLUDED.password_hash,
                  updated_at = now()
            RETURNING id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(upsertUserSql, connection);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@passwordHash", passwordHash);

        var userId = Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
        _customers[normalizedAlias] = new TestCustomer(userId, email, password);
    }

    [Given("customer {string} has an active order {string}")]
    public Task GivenCustomerHasAnActiveOrderAsync(string alias, string orderId) =>
        SeedOrderAsync(alias, orderId, isSoftDeleted: false);

    [Given("customer {string} has a soft deleted order {string}")]
    public Task GivenCustomerHasASoftDeletedOrderAsync(string alias, string orderId) =>
        SeedOrderAsync(alias, orderId, isSoftDeleted: true);

    [When("I authenticate through the API as {string}")]
    public async Task WhenIAuthenticateThroughTheApiAsAsync(string alias)
    {
        var customer = GetCustomer(alias);

        using var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new
        {
            Email = customer.Email,
            Password = customer.Password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await ReadJsonAsync(response);
        json.RootElement.TryGetProperty("accessToken", out var tokenProperty).Should().BeTrue();

        var token = tokenProperty.GetString();
        token.Should().NotBeNullOrWhiteSpace();

        customer.AccessToken = token!;
    }

    [When("I GET {string} without a JWT token")]
    public Task WhenIGetWithoutAJwtTokenAsync(string path) =>
        SendGetAsync(path, alias: null, includeJwt: false);

    [When("I GET {string} as customer {string}")]
    public Task WhenIGetAsCustomerAsync(string path, string alias) =>
        SendGetAsync(path, alias, includeJwt: true);

    [When("I GET MVC page {string} without authentication")]
    public Task WhenIGetMvcPageWithoutAuthenticationAsync(string path) =>
        SendGetAsync(path, alias: null, includeJwt: false);

    [When("I GET MVC page {string} with the authenticated session for {string}")]
    public Task WhenIGetMvcPageWithAuthenticatedSessionForAsync(string path, string alias) =>
        SendGetAsync(path, alias, includeJwt: false);

    [Then("the HTTP status code should be {int}")]
    public void ThenTheHttpStatusCodeShouldBe(int expectedStatusCode)
    {
        _lastResponse.Should().NotBeNull();
        ((int)_lastResponse!.StatusCode).Should().Be(expectedStatusCode);
    }

    [Then("the orders payload should only contain customer {string} orders")]
    public void ThenTheOrdersPayloadShouldOnlyContainCustomerOrders(string alias)
    {
        var customerId = GetCustomer(alias).Id.ToString(CultureInfo.InvariantCulture);
        var ids = GetOrdersPayloadCustomerIds();

        ids.Should().NotBeEmpty();
        ids.Should().OnlyContain(id => string.Equals(id, customerId, StringComparison.Ordinal));
    }

    [Then("the orders payload should contain ids {string}")]
    public void ThenTheOrdersPayloadShouldContainIds(string commaSeparatedOrderIds)
    {
        var expectedIds = SplitCsv(commaSeparatedOrderIds);
        var actualIds = GetOrdersPayloadOrderIds();

        foreach (var expectedId in expectedIds)
            actualIds.Should().Contain(expectedId);
    }

    [Then("the orders payload should not contain ids {string}")]
    public void ThenTheOrdersPayloadShouldNotContainIds(string commaSeparatedOrderIds)
    {
        var forbiddenIds = SplitCsv(commaSeparatedOrderIds);
        var actualIds = GetOrdersPayloadOrderIds();

        foreach (var forbiddenId in forbiddenIds)
            actualIds.Should().NotContain(forbiddenId);
    }

    [Then("the order payload id should be {string}")]
    public void ThenTheOrderPayloadIdShouldBe(string expectedOrderId)
    {
        var root = GetLastJsonRoot();
        root.GetProperty("id").GetString().Should().Be(expectedOrderId);
    }

    [Then("the order payload customer should be {string}")]
    public void ThenTheOrderPayloadCustomerShouldBe(string alias)
    {
        var expectedCustomerId = GetCustomer(alias).Id.ToString(CultureInfo.InvariantCulture);
        var root = GetLastJsonRoot();

        root.GetProperty("customerId").GetString().Should().Be(expectedCustomerId);
    }

    [Then("the response should include text {string}")]
    public async Task ThenTheResponseShouldIncludeTextAsync(string expectedText)
    {
        _lastResponse.Should().NotBeNull();
        var responseText = await _lastResponse!.Content.ReadAsStringAsync();
        responseText.Should().Contain(expectedText);
    }

    [Then("the response should redirect to {string}")]
    public void ThenTheResponseShouldRedirectTo(string targetPath)
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.Headers.Location.Should().NotBeNull();
        _lastResponse.Headers.Location!.ToString().Should().ContainEquivalentOf(targetPath);
    }

    [AfterScenario]
    public async Task AfterScenarioAsync()
    {
        await CleanupSeedDataAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _lastJson?.Dispose();
        _lastResponse?.Dispose();
        _httpClient.Dispose();
        await _dataSource.DisposeAsync();
    }

    private async Task SeedOrderAsync(string alias, string orderId, bool isSoftDeleted)
    {
        var customer = GetCustomer(alias);

        const string upsertOrderSql = """
            INSERT INTO domain.orders (
                id,
                customer_id,
                order_date,
                shipping_address,
                total_amount,
                currency,
                created_at,
                updated_at,
                deleted_at)
            VALUES (
                @id,
                @customerId,
                now() - interval '1 day',
                'Integration Test Address',
                99.99,
                'USD',
                now(),
                now(),
                @deletedAt)
            ON CONFLICT (id) DO UPDATE
              SET customer_id = EXCLUDED.customer_id,
                  order_date = EXCLUDED.order_date,
                  shipping_address = EXCLUDED.shipping_address,
                  total_amount = EXCLUDED.total_amount,
                  currency = EXCLUDED.currency,
                  updated_at = now(),
                  deleted_at = EXCLUDED.deleted_at;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(upsertOrderSql, connection);
        command.Parameters.AddWithValue("@id", orderId);
        command.Parameters.AddWithValue("@customerId", customer.Id);
        command.Parameters.AddWithValue("@deletedAt", isSoftDeleted ? DateTimeOffset.UtcNow : DBNull.Value);

        await command.ExecuteNonQueryAsync();
        _managedOrderIds.Add(orderId);
    }

    private async Task SendGetAsync(string path, string? alias, bool includeJwt)
    {
        _lastJson?.Dispose();
        _lastJson = null;

        _lastResponse?.Dispose();
        _lastResponse = null;

        using var request = new HttpRequestMessage(HttpMethod.Get, path);

        if (includeJwt && !string.IsNullOrWhiteSpace(alias))
        {
            var customer = GetCustomer(alias);
            customer.AccessToken.Should().NotBeNullOrWhiteSpace("customer must authenticate before calling JWT-protected endpoints");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", customer.AccessToken);
        }

        _lastResponse = await _httpClient.SendAsync(request);
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    private JsonElement GetLastJsonRoot()
    {
        _lastResponse.Should().NotBeNull();

        if (_lastJson is not null)
            return _lastJson.RootElement;

        var content = _lastResponse!.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        _lastJson = JsonDocument.Parse(content);
        return _lastJson.RootElement;
    }

    private List<string> GetOrdersPayloadOrderIds()
    {
        var root = GetLastJsonRoot();
        var data = root.GetProperty("data");

        return data.EnumerateArray()
            .Select(item => item.GetProperty("id").GetString())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .ToList();
    }

    private List<string> GetOrdersPayloadCustomerIds()
    {
        var root = GetLastJsonRoot();
        var data = root.GetProperty("data");

        return data.EnumerateArray()
            .Select(item => item.GetProperty("customerId").GetString())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .ToList();
    }

    private async Task CleanupSeedDataAsync()
    {
        var userIds = _customers.Values
            .Select(customer => customer.Id)
            .Distinct()
            .ToArray();

        var orderIds = _managedOrderIds.ToArray();

        await using var connection = await _dataSource.OpenConnectionAsync();

        if (orderIds.Length > 0)
        {
            const string deleteOrdersSql = "DELETE FROM domain.orders WHERE id = ANY(@orderIds);";
            await using var deleteOrdersCommand = new NpgsqlCommand(deleteOrdersSql, connection);
            deleteOrdersCommand.Parameters.AddWithValue("@orderIds", orderIds);
            await deleteOrdersCommand.ExecuteNonQueryAsync();
        }

        if (userIds.Length > 0)
        {
            const string deleteUsersSql = "DELETE FROM auth.users WHERE id = ANY(@userIds);";
            await using var deleteUsersCommand = new NpgsqlCommand(deleteUsersSql, connection);
            deleteUsersCommand.Parameters.AddWithValue("@userIds", userIds);
            await deleteUsersCommand.ExecuteNonQueryAsync();
        }
    }

    private static string NormalizeAlias(string alias)
    {
        return alias.Trim().ToLowerInvariant();
    }

    private TestCustomer GetCustomer(string alias)
    {
        var normalizedAlias = NormalizeAlias(alias);
        _customers.TryGetValue(normalizedAlias, out var customer).Should().BeTrue($"customer '{alias}' was not seeded");
        return customer!;
    }

    private static string[] SplitCsv(string csv)
    {
        return csv
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToArray();
    }

    private sealed class TestCustomer
    {
        public TestCustomer(int id, string email, string password)
        {
            Id = id;
            Email = email;
            Password = password;
        }

        public int Id { get; }
        public string Email { get; }
        public string Password { get; }
        public string? AccessToken { get; set; }
    }
}

using BLA.Ordering.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace BLA.Ordering.Web.API.Tests.TestInfrastructure;

public sealed class OrdersApiTestFixture : IAsyncLifetime
{
    private const string JwtIssuer = "bla-tests";
    private const string JwtAudience = "bla-tests-audience";
    private const string JwtKey = "0123456789ABCDEF0123456789ABCDEF";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("bla_ordering_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await InitializeSchemaAsync();
        await SeedDataAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    var inMemoryConfig = new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Postgres"] = _container.GetConnectionString(),
                        ["Jwt:Issuer"] = JwtIssuer,
                        ["Jwt:Audience"] = JwtAudience,
                        ["Jwt:Key"] = JwtKey,
                        ["Jwt:AccessTokenMinutes"] = "60"
                    };

                    configBuilder.AddInMemoryCollection(inMemoryConfig);
                });

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<NpgsqlDataSource>();
                    services.AddSingleton(_ => NpgsqlDataSource.Create(_container.GetConnectionString()));

                    services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                    {
                        options.TokenValidationParameters.ValidateIssuer = false;
                        options.TokenValidationParameters.ValidateAudience = false;
                        options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                        options.TokenValidationParameters.RequireSignedTokens = false;
                    });

                    services.PostConfigure<AuthorizationOptions>(options =>
                    {
                        options.AddPolicy("ApiJwtPolicy", policy =>
                        {
                            policy.RequireAuthenticatedUser();
                        });
                    });
                });
            });
    }

    public async Task DisposeAsync()
    {
        Factory.Dispose();
        await _container.DisposeAsync();
    }

    private async Task InitializeSchemaAsync()
    {
        const string createAuthSchemaSql = """
            CREATE SCHEMA IF NOT EXISTS auth;

            CREATE TABLE IF NOT EXISTS auth.users (
                id            SERIAL PRIMARY KEY,
                email         VARCHAR(254)  NOT NULL,
                password_hash VARCHAR(255)  NOT NULL,
                created_at    TIMESTAMPTZ   NOT NULL DEFAULT now(),
                updated_at    TIMESTAMPTZ   NOT NULL DEFAULT now(),

                CONSTRAINT ck_users_email_not_empty CHECK (char_length(trim(email)) > 0),
                CONSTRAINT ck_users_password_hash_not_empty CHECK (char_length(password_hash) > 0)
            );

            CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON auth.users (email);
            """;

        const string createOrdersSql = """
            CREATE SCHEMA IF NOT EXISTS domain;

            CREATE TABLE IF NOT EXISTS domain.orders (
                id               VARCHAR(120)   PRIMARY KEY,
                customer_id      INTEGER        NOT NULL,
                order_date       TIMESTAMPTZ    NOT NULL,
                shipping_address VARCHAR(300)   NOT NULL,
                total_amount     NUMERIC(18,4)  NOT NULL,
                currency         VARCHAR(3)     NOT NULL,
                created_at       TIMESTAMPTZ    NOT NULL DEFAULT now(),
                updated_at       TIMESTAMPTZ    NOT NULL DEFAULT now(),
                deleted_at       TIMESTAMPTZ    NULL,

                CONSTRAINT fk_orders_customer
                    FOREIGN KEY (customer_id)
                    REFERENCES auth.users(id),
                CONSTRAINT ck_orders_shipping_address_not_empty
                    CHECK (char_length(trim(shipping_address)) > 0),
                CONSTRAINT ck_orders_total_amount_positive
                    CHECK (total_amount > 0),
                CONSTRAINT ck_orders_currency_allowed
                    CHECK (currency IN ('USD', 'EUR'))
            );

            CREATE INDEX IF NOT EXISTS idx_orders_customer_active
                ON domain.orders (customer_id, updated_at DESC)
                WHERE deleted_at IS NULL;
            """;

        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();

        await using (var command = new NpgsqlCommand(createAuthSchemaSql, connection))
            await command.ExecuteNonQueryAsync();

        await using (var command = new NpgsqlCommand(createOrdersSql, connection))
            await command.ExecuteNonQueryAsync();
    }

    private async Task SeedDataAsync()
    {
        var passwordHasher = new BcryptPasswordHasher();
        var passwordHash = passwordHasher.Hash("Password1!");

        const string seedSql = """
            INSERT INTO auth.users (id, email, password_hash, created_at, updated_at)
            VALUES
                (1001, 'user1@example.com', @passwordHash, now(), now()),
                (1002, 'user2@example.com', @passwordHash, now(), now())
            ON CONFLICT (id) DO NOTHING;

            UPDATE auth.users
            SET password_hash = @passwordHash,
                updated_at = now()
            WHERE id IN (1001, 1002);

            INSERT INTO domain.orders (id, customer_id, order_date, shipping_address, total_amount, currency, created_at, updated_at, deleted_at)
            VALUES
                ('ORD-U1-1', 1001, now() - interval '1 day', '123 Alpha St', 49.99, 'USD', now(), now(), NULL),
                ('ORD-U1-2', 1001, now() - interval '2 days', '123 Alpha St', 79.50, 'EUR', now(), now(), NULL),
                ('ORD-U2-1', 1002, now() - interval '1 day', '987 Beta Ave', 15.25, 'USD', now(), now(), NULL),
                ('ORD-U1-DELETED', 1001, now() - interval '3 days', '123 Alpha St', 10.00, 'USD', now(), now(), now())
            ON CONFLICT (id) DO NOTHING;
            """;

        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(seedSql, connection);
        command.Parameters.AddWithValue("@passwordHash", passwordHash);
        await command.ExecuteNonQueryAsync();

        await using var verifyCommand = new NpgsqlCommand("SELECT password_hash FROM auth.users WHERE id = 1001", connection);
        var persistedHash = Convert.ToString(await verifyCommand.ExecuteScalarAsync());

        if (string.IsNullOrWhiteSpace(persistedHash) || !passwordHasher.Verify("Password1!", persistedHash))
            throw new InvalidOperationException("Failed to seed valid authentication credentials for integration tests.");
    }
}

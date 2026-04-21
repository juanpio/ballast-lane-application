using BLA.Ordering.Application.Auth;
using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Application.Auth.Validators;
using BLA.Ordering.Application.Orders.Commands;
using BLA.Ordering.Application.Orders.Queries;
using BLA.Ordering.Application.Orders.Validators;
using BLA.Ordering.Domain.Interfaces;
using BLA.Ordering.Infrastructure.Auth;
using BLA.Ordering.Infrastructure.Persistence.Repositories;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) =>
    {
        cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "BLA.Ordering");
    });

    // Database
    var connectionString = builder.Configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
    builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));

    // Authentication
    var jwtIssuer = builder.Configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
    var jwtAudience = builder.Configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key is not configured.");

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "bla.session";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.LoginPath = "/account/login";
            options.AccessDeniedPath = "/account/login";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ApiJwtPolicy", policy =>
        {
            policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
        });
    });

    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new HeaderApiVersionReader("api-version"),
                new UrlSegmentApiVersionReader());
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    // Infrastructure
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

    // Application
    builder.Services.AddScoped<RegisterUserCommandHandler>();
    builder.Services.AddScoped<RegisterUserValidator>();
    builder.Services.AddScoped<AuthenticateUserCommandHandler>();
    builder.Services.AddScoped<AuthenticateUserValidator>();
    builder.Services.AddScoped<CreateOrderCommandHandler>();
    builder.Services.AddScoped<UpdateOrderCommandHandler>();
    builder.Services.AddScoped<DeleteOrderCommandHandler>();
    builder.Services.AddScoped<GetOrderByIdQueryHandler>();
    builder.Services.AddScoped<GetOrdersQueryHandler>();
    builder.Services.AddScoped<CreateOrderValidator>();
    builder.Services.AddScoped<UpdateOrderValidator>();
    builder.Services.AddScoped<DeleteOrderValidator>();

    // Web
    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        };
    });

    app.Use(async (context, next) =>
    {
        // Support URL segment versioning centrally: /api/v{version}/... -> /api/...
        var path = context.Request.Path.Value;
        if (!string.IsNullOrWhiteSpace(path)
            && path.StartsWith("/api/v", StringComparison.OrdinalIgnoreCase))
        {
            var versionAndRemainder = path["/api/v".Length..];
            var separatorIndex = versionAndRemainder.IndexOf('/');
            if (separatorIndex > 0)
            {
                var requestedVersion = versionAndRemainder[..separatorIndex];
                if (Version.TryParse(requestedVersion, out _))
                {
                    if (!context.Request.Headers.ContainsKey("api-version"))
                        context.Request.Headers["api-version"] = requestedVersion;

                    var rewrittenPath = "/api" + versionAndRemainder[separatorIndex..];
                    context.Request.Path = rewrittenPath;
                }
            }
        }

        // Support legacy header name during transition.
        if (!context.Request.Headers.ContainsKey("api-version")
            && context.Request.Headers.TryGetValue("x-version", out var legacyApiVersion)
            && !string.IsNullOrWhiteSpace(legacyApiVersion.ToString()))
        {
            context.Request.Headers["api-version"] = legacyApiVersion.ToString();
        }

        await next();
    });

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // Public landing page: registration
    app.MapGet("/", () => Results.Redirect("/account/create", permanent: false));

    // Support attribute-routed controllers like /account/create.
    app.MapControllers();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Register}/{id?}");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application start-up failed.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;


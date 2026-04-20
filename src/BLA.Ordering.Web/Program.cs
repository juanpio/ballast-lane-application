using BLA.Ordering.Application.Auth;
using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Application.Auth.Validators;
using BLA.Ordering.Domain.Interfaces;
using BLA.Ordering.Infrastructure.Auth;
using BLA.Ordering.Infrastructure.Persistence.Repositories;
using Npgsql;
using Serilog;

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

    // Infrastructure
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

    // Application
    builder.Services.AddScoped<RegisterUserCommandHandler>();
    builder.Services.AddScoped<RegisterUserValidator>();

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

    app.UseStaticFiles();
    app.UseRouting();

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


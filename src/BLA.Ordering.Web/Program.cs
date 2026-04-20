using BLA.Ordering.Application.Auth;
using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Application.Auth.Validators;
using BLA.Ordering.Domain.Interfaces;
using BLA.Ordering.Infrastructure.Auth;
using BLA.Ordering.Infrastructure.Persistence.Repositories;
using Npgsql;
var builder = WebApplication.CreateBuilder(args);
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

    app.UseStaticFiles();
    app.UseRouting();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

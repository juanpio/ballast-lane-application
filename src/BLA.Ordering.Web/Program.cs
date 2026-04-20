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

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

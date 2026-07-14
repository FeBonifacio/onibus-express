using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnibusExpress.Infrastructure.Persistence;

namespace OnibusExpress.Tests.Integration;

/// <summary>
/// Boots the real API in-process, swapping PostgreSQL for a SQLite in-memory database
/// (the challenge's suggested approach). The connection is kept open for the factory's
/// lifetime so the in-memory schema/data survive between requests.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // Replace the Npgsql-configured DbContext with SQLite.
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                .ToList();
            foreach (var d in toRemove)
            {
                services.Remove(d);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }

    /// <summary>Seeds a route + trip and returns the trip id, for tests to act on.</summary>
    public async Task<Guid> SeedTripAsync(int totalSeats = 44, TimeSpan? untilDeparture = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var route = Route.Create(Guid.NewGuid(), "Sao Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        var trip = Trip.Create(Guid.NewGuid(), route.Id, route.Origin, route.Destination,
            clock.UtcNow.Add(untilDeparture ?? TimeSpan.FromDays(1)), 120m, totalSeats);

        db.Routes.Add(route);
        db.Trips.Add(trip);
        await db.SaveChangesAsync();
        return trip.Id;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}

namespace OnibusExpress.Infrastructure.Persistence;

/// <summary>
/// Idempotent seed. Routes are inserted once (when absent). Trips are (re)seeded whenever there
/// are no UPCOMING trips, so a demo always has bookable trips even if a persisted database has
/// grown stale over time. Departures are relative to <see cref="IClock"/> (always in the future).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IClock clock, CancellationToken ct = default)
    {
        var now = clock.UtcNow;

        if (!await db.Routes.AnyAsync(ct))
        {
            db.Routes.AddRange(
                Route.Create(Guid.NewGuid(), "Sao Paulo", "Rio de Janeiro", TimeSpan.FromHours(6)),
                Route.Create(Guid.NewGuid(), "Sao Paulo", "Belo Horizonte", TimeSpan.FromHours(8)),
                Route.Create(Guid.NewGuid(), "Curitiba", "Florianopolis", TimeSpan.FromHours(5)));
            await db.SaveChangesAsync(ct);
        }

        var hasUpcomingTrips = await db.Trips.AnyAsync(t => t.DepartureUtc > now, ct);
        if (hasUpcomingTrips)
        {
            return;
        }

        var routes = await db.Routes.ToListAsync(ct);
        foreach (var route in routes)
        {
            db.Trips.AddRange(
                Trip.Create(Guid.NewGuid(), route.Id, route.Origin, route.Destination, now.AddDays(1).AddHours(8), 120m, 44),
                Trip.Create(Guid.NewGuid(), route.Id, route.Origin, route.Destination, now.AddDays(1).AddHours(14), 135m, 44),
                Trip.Create(Guid.NewGuid(), route.Id, route.Origin, route.Destination, now.AddDays(2).AddHours(9), 120m, 40));
        }

        await db.SaveChangesAsync(ct);
    }
}

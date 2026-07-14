namespace OnibusExpress.Infrastructure.Repositories;

public sealed class EfTripRepository : ITripRepository
{
    private readonly AppDbContext _db;

    public EfTripRepository(AppDbContext db) => _db = db;

    public async Task<Trip?> GetWithReservationsAsync(Guid id, CancellationToken ct) =>
        await _db.Trips
            .Include(t => t.Reservations)
            .ThenInclude(r => r.Passenger)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Trip>> SearchAsync(string? origin, string? destination, DateOnly? date, CancellationToken ct)
    {
        var query = _db.Trips.Include(t => t.Reservations).AsQueryable();

        if (!string.IsNullOrWhiteSpace(origin))
        {
            var o = origin.Trim().ToLower();
            query = query.Where(t => t.Origin.ToLower() == o);
        }

        if (!string.IsNullOrWhiteSpace(destination))
        {
            var d = destination.Trim().ToLower();
            query = query.Where(t => t.Destination.ToLower() == d);
        }

        if (date is not null)
        {
            var start = new DateTimeOffset(date.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var end = start.AddDays(1);
            query = query.Where(t => t.DepartureUtc >= start && t.DepartureUtc < end);
        }

        // Order client-side: SQLite cannot ORDER BY DateTimeOffset (works on PostgreSQL,
        // but we keep it portable so the integration tests can run on SQLite in-memory).
        var trips = await query.ToListAsync(ct);
        return trips.OrderBy(t => t.DepartureUtc).ToList();
    }

    // The trip is tracked (loaded via GetWithReservationsAsync); the new Reservation and its
    // Passenger are detected on SaveChanges. Kept for symmetry / explicit intent.
    public Task UpdateAsync(Trip trip, CancellationToken ct)
    {
        if (_db.Entry(trip).State == EntityState.Detached)
        {
            _db.Trips.Update(trip);
        }

        return Task.CompletedTask;
    }
}

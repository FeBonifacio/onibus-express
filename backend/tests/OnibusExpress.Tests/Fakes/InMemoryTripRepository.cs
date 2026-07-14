namespace OnibusExpress.Tests.Fakes;

public sealed class InMemoryTripRepository : ITripRepository
{
    private readonly InMemoryDatabase _db;

    public InMemoryTripRepository(InMemoryDatabase db) => _db = db;

    /// <summary>How many times <see cref="UpdateAsync"/> was called (persistence asserts).</summary>
    public int UpdateCalls { get; private set; }

    public Task<Trip?> GetWithReservationsAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_db.Trips.FirstOrDefault(t => t.Id == id));

    public Task<IReadOnlyList<Trip>> SearchAsync(string? origin, string? destination, DateOnly? date, CancellationToken ct)
    {
        IEnumerable<Trip> q = _db.Trips;

        if (!string.IsNullOrWhiteSpace(origin))
        {
            q = q.Where(t => t.Origin.Equals(origin.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(destination))
        {
            q = q.Where(t => t.Destination.Equals(destination.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (date is not null)
        {
            q = q.Where(t => DateOnly.FromDateTime(t.DepartureUtc.UtcDateTime) == date.Value);
        }

        return Task.FromResult<IReadOnlyList<Trip>>(q.ToList());
    }

    public Task UpdateAsync(Trip trip, CancellationToken ct)
    {
        UpdateCalls++;
        if (_db.Trips.All(t => t.Id != trip.Id))
        {
            _db.Trips.Add(trip);
        }

        return Task.CompletedTask;
    }
}

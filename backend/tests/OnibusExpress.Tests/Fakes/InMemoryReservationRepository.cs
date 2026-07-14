namespace OnibusExpress.Tests.Fakes;

/// <summary>
/// Reservation repository for read/cancellation. Reservations live inside the trips of
/// the shared <see cref="InMemoryDatabase"/>. Supports a "collision mode" to exercise the
/// code-generation retry.
/// </summary>
public sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly InMemoryDatabase _db;
    private int _collisionsLeft;

    public InMemoryReservationRepository(InMemoryDatabase db) => _db = db;

    public int UpdateCalls { get; private set; }

    /// <summary>Forces <see cref="CodeExistsAsync"/> to return true for the next N calls.</summary>
    public void ForceCollisions(int n) => _collisionsLeft = n;

    private IEnumerable<Reservation> AllReservations => _db.Trips.SelectMany(t => t.Reservations);

    public Task<Reservation?> GetByCodeAsync(ReservationCode code, CancellationToken ct) =>
        Task.FromResult(AllReservations.FirstOrDefault(r => r.Code == code));

    public Task<bool> CodeExistsAsync(ReservationCode code, CancellationToken ct)
    {
        if (_collisionsLeft > 0)
        {
            _collisionsLeft--;
            return Task.FromResult(true);
        }
        return Task.FromResult(AllReservations.Any(r => r.Code == code));
    }

    public Task UpdateAsync(Reservation reservation, CancellationToken ct)
    {
        UpdateCalls++; // the reservation is already the same instance inside the trip
        return Task.CompletedTask;
    }
}

namespace OnibusExpress.Infrastructure.Repositories;

public sealed class EfReservationRepository : IReservationRepository
{
    private readonly AppDbContext _db;

    public EfReservationRepository(AppDbContext db) => _db = db;

    public async Task<Reservation?> GetByCodeAsync(ReservationCode code, CancellationToken ct) =>
        await _db.Reservations
            .Include(r => r.Passenger)
            .FirstOrDefaultAsync(r => r.Code == code, ct);

    public async Task<bool> CodeExistsAsync(ReservationCode code, CancellationToken ct) =>
        await _db.Reservations.AnyAsync(r => r.Code == code, ct);

    // The reservation is tracked (loaded via GetByCodeAsync); Cancel mutates it and
    // SaveChanges persists the change.
    public Task UpdateAsync(Reservation reservation, CancellationToken ct) => Task.CompletedTask;
}

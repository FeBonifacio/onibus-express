using Npgsql;

namespace OnibusExpress.Infrastructure.Persistence;

/// <summary>
/// Commits via <c>SaveChangesAsync</c> and translates unique-index violations into domain
/// exceptions — the DB is the arbiter of the two concurrency races (seat and reservation code)
/// documented in Phase 2. Provider-agnostic: handles PostgreSQL (SQLSTATE 23505) and SQLite
/// (used by the integration tests) without a production dependency on the SQLite provider.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    private enum UniqueViolation { Seat, Code }

    private readonly AppDbContext _db;

    public EfUnitOfWork(AppDbContext db) => _db = db;

    public async Task CommitAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (TryClassify(ex, out var violation))
        {
            throw violation == UniqueViolation.Seat
                ? new SeatTakenException()
                : new ReservationCodeDuplicateException();
        }
    }

    private static bool TryClassify(DbUpdateException ex, out UniqueViolation violation)
    {
        violation = UniqueViolation.Code;
        var inner = ex.InnerException;

        // PostgreSQL: unique_violation, disambiguated by constraint name.
        if (inner is PostgresException { SqlState: "23505" } pg)
        {
            violation = pg.ConstraintName == "IX_Reservations_Trip_Seat_Active"
                ? UniqueViolation.Seat
                : UniqueViolation.Code;
            return true;
        }

        // SQLite (integration tests): match by type name + message, no hard reference to the provider.
        if (inner is not null
            && inner.GetType().Name == "SqliteException"
            && inner.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
        {
            violation = inner.Message.Contains("Seat", StringComparison.OrdinalIgnoreCase)
                ? UniqueViolation.Seat
                : UniqueViolation.Code;
            return true;
        }

        return false;
    }
}

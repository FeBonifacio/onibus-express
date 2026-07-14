namespace OnibusExpress.Application.Abstractions;

public interface ITripRepository
{
    /// <summary>Loads the full aggregate (trip + its reservations).</summary>
    Task<Trip?> GetWithReservationsAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<Trip>> SearchAsync(string? origin, string? destination, DateOnly? date, CancellationToken ct);

    /// <summary>Single persistence point for a new Reservation: the aggregate root saves its child.</summary>
    Task UpdateAsync(Trip trip, CancellationToken ct);
}

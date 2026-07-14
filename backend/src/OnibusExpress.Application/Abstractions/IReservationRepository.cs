namespace OnibusExpress.Application.Abstractions;

/// <summary>
/// Reservations for READ and CANCELLATION. New reservations are created through the
/// aggregate root (<see cref="ITripRepository.UpdateAsync"/>). The DepartureUtc snapshot
/// on the Reservation lets us cancel by code without loading the whole Trip.
/// </summary>
public interface IReservationRepository
{
    Task<Reservation?> GetByCodeAsync(ReservationCode code, CancellationToken ct);
    Task<bool> CodeExistsAsync(ReservationCode code, CancellationToken ct);
    Task UpdateAsync(Reservation reservation, CancellationToken ct);
}

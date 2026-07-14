namespace OnibusExpress.Application.UseCases.Reservations;

/// <summary>DELETE /reservas/{codigo} — cancels (the 2h rule lives in <see cref="Reservation.Cancel"/>).</summary>
public sealed class CancelReservationUseCase
{
    private readonly IReservationRepository _reservations;
    private readonly IClock _clock;
    private readonly IUnitOfWork _uow;

    public CancelReservationUseCase(IReservationRepository reservations, IClock clock, IUnitOfWork uow)
    {
        _reservations = reservations;
        _clock = clock;
        _uow = uow;
    }

    public async Task ExecuteAsync(string code, CancellationToken ct)
    {
        var reservationCode = ReservationCode.Create(code); // 400 if malformed
        var reservation = await _reservations.GetByCodeAsync(reservationCode, ct)
                          ?? throw new NotFoundException("Reserva", code);

        reservation.Cancel(_clock); // 409 too late / already cancelled

        await _reservations.UpdateAsync(reservation, ct);
        await _uow.CommitAsync(ct);
    }
}

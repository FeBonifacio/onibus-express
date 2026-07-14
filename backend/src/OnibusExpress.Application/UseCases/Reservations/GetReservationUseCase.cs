namespace OnibusExpress.Application.UseCases.Reservations;

/// <summary>GET /reservas/{codigo} — looks up a reservation by code.</summary>
public sealed class GetReservationUseCase
{
    private readonly IReservationRepository _reservations;

    public GetReservationUseCase(IReservationRepository reservations) => _reservations = reservations;

    public async Task<ReservationResponse> ExecuteAsync(string code, CancellationToken ct)
    {
        var reservationCode = ReservationCode.Create(code); // 400 if malformed
        var reservation = await _reservations.GetByCodeAsync(reservationCode, ct)
                          ?? throw new NotFoundException("Reserva", code);
        return ReservationMapper.ToResponse(reservation);
    }
}

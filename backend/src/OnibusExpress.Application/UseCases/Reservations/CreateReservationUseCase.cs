namespace OnibusExpress.Application.UseCases.Reservations;

/// <summary>
/// POST /reservas. The business invariants (seat taken, trip departed) live in
/// <see cref="Trip.Reserve"/> — this use case orchestrates input validation, unique
/// code generation and persistence through the aggregate root.
/// </summary>
public sealed class CreateReservationUseCase
{
    private const int MaxCodeAttempts = 5;

    private readonly ITripRepository _trips;
    private readonly IReservationRepository _reservations;
    private readonly IReservationCodeGenerator _codeGenerator;
    private readonly IClock _clock;
    private readonly IUnitOfWork _uow;

    public CreateReservationUseCase(
        ITripRepository trips,
        IReservationRepository reservations,
        IReservationCodeGenerator codeGenerator,
        IClock clock,
        IUnitOfWork uow)
    {
        _trips = trips;
        _reservations = reservations;
        _codeGenerator = codeGenerator;
        _clock = clock;
        _uow = uow;
    }

    public async Task<ReservationResponse> ExecuteAsync(CreateReservationRequest request, CancellationToken ct)
    {
        var document = Document.Create(request.Document);     // 400 if invalid
        var email = Email.Create(request.Email);              // 400 if invalid
        var seat = SeatNumber.Create(request.Seat);           // 400 if < 1

        var trip = await _trips.GetWithReservationsAsync(request.TripId, ct)
                   ?? throw new NotFoundException("Viagem", request.TripId);

        var passenger = Passenger.Create(Guid.NewGuid(), request.Name, document, email, request.BirthDate, _clock);
        var code = await GenerateUniqueCodeAsync(ct);

        var reservation = trip.Reserve(passenger, seat, code, _clock); // rules 1 and 2

        await _trips.UpdateAsync(trip, ct); // persist the reservation via the aggregate root (single write)
        await _uow.CommitAsync(ct);
        return ReservationMapper.ToResponse(reservation);
    }

    private async Task<ReservationCode> GenerateUniqueCodeAsync(CancellationToken ct)
    {
        for (int i = 0; i < MaxCodeAttempts; i++)
        {
            var candidate = _codeGenerator.Generate();
            if (!await _reservations.CodeExistsAsync(candidate, ct))
            {
                return candidate;
            }
        }
        throw new CouldNotGenerateCodeException(MaxCodeAttempts);
    }
}

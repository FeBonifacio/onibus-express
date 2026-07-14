namespace OnibusExpress.Application.UseCases.Trips;

/// <summary>GET /viagens/{id} — detail with free/taken seats.</summary>
public sealed class GetTripUseCase
{
    private readonly ITripRepository _trips;

    public GetTripUseCase(ITripRepository trips) => _trips = trips;

    public async Task<TripDetailResponse> ExecuteAsync(Guid id, CancellationToken ct)
    {
        var trip = await _trips.GetWithReservationsAsync(id, ct)
                   ?? throw new NotFoundException("Viagem", id);
        return TripMapper.ToDetail(trip);
    }
}

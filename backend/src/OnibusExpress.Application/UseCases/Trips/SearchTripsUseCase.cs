namespace OnibusExpress.Application.UseCases.Trips;

/// <summary>GET /viagens — searches by origin, destination and date.</summary>
public sealed class SearchTripsUseCase
{
    private readonly ITripRepository _trips;

    public SearchTripsUseCase(ITripRepository trips) => _trips = trips;

    public async Task<IReadOnlyList<TripSummaryResponse>> ExecuteAsync(SearchTripsQuery query, CancellationToken ct)
    {
        var trips = await _trips.SearchAsync(query.Origin, query.Destination, query.Date, ct);
        return trips.Select(TripMapper.ToSummary).ToList();
    }
}

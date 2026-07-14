namespace OnibusExpress.Application.Mapping;

public static class TripMapper
{
    public static TripSummaryResponse ToSummary(Trip t) =>
        new(t.Id, t.Origin, t.Destination, t.DepartureUtc, t.BasePrice, t.TotalSeats, t.AvailableSeats);

    public static TripDetailResponse ToDetail(Trip t) =>
        new(t.Id, t.Origin, t.Destination, t.DepartureUtc, t.BasePrice, t.TotalSeats,
            t.AvailableSeats, t.TakenSeats(), t.FreeSeats());
}

namespace OnibusExpress.Application.Mapping;

public static class ReservationMapper
{
    public static ReservationResponse ToResponse(Reservation r) =>
        new(
            r.Code.Value,
            r.TripId,
            r.Passenger.Name,
            r.Passenger.Document.Formatted,
            r.Seat.Value,
            r.Status.ToString(),
            r.DepartureUtc,
            r.Price);
}

namespace OnibusExpress.Application.DTOs;

public sealed record ReservationResponse(
    string Code,
    Guid TripId,
    string PassengerName,
    string DocumentFormatted,
    int Seat,
    string Status,
    DateTimeOffset DepartureUtc,
    decimal Price);

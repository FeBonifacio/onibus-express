namespace OnibusExpress.Application.DTOs;

public sealed record TripSummaryResponse(
    Guid Id,
    string Origin,
    string Destination,
    DateTimeOffset DepartureUtc,
    decimal BasePrice,
    int TotalSeats,
    int AvailableSeats);

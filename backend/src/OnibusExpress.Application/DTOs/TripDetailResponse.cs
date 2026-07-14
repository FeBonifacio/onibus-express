namespace OnibusExpress.Application.DTOs;

public sealed record TripDetailResponse(
    Guid Id,
    string Origin,
    string Destination,
    DateTimeOffset DepartureUtc,
    decimal BasePrice,
    int TotalSeats,
    int AvailableSeats,
    IReadOnlyList<int> TakenSeats,
    IReadOnlyList<int> FreeSeats);

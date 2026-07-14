namespace OnibusExpress.Application.DTOs;

public sealed record CreateReservationRequest(
    Guid TripId,
    string Name,
    string Document,
    string Email,
    DateOnly BirthDate,
    int Seat);

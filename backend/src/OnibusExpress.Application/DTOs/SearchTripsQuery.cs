namespace OnibusExpress.Application.DTOs;

public sealed record SearchTripsQuery(string? Origin, string? Destination, DateOnly? Date);

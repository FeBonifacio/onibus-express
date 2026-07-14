namespace OnibusExpress.Application.DTOs;

public sealed record RouteResponse(Guid Id, string Origin, string Destination, TimeSpan EstimatedDuration);

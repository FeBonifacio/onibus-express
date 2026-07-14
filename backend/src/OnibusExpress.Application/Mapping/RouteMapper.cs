namespace OnibusExpress.Application.Mapping;

public static class RouteMapper
{
    public static RouteResponse ToResponse(Route r) =>
        new(r.Id, r.Origin, r.Destination, r.EstimatedDuration);
}

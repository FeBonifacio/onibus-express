namespace OnibusExpress.Tests.Builders;

/// <summary>Fluent <see cref="Trip"/> builder with sensible defaults (future departure).</summary>
public sealed class TripBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _routeId = Guid.NewGuid();
    private string _origin = "Sao Paulo";
    private string _destination = "Rio de Janeiro";
    private DateTimeOffset _departure = new(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private decimal _price = 120m;
    private int _totalSeats = 44;

    public TripBuilder WithId(Guid id) { _id = id; return this; }
    public TripBuilder WithRouteId(Guid routeId) { _routeId = routeId; return this; }
    public TripBuilder WithOrigin(string origin) { _origin = origin; return this; }
    public TripBuilder WithDestination(string destination) { _destination = destination; return this; }
    public TripBuilder WithDeparture(DateTimeOffset departure) { _departure = departure; return this; }
    public TripBuilder WithPrice(decimal price) { _price = price; return this; }
    public TripBuilder WithTotalSeats(int totalSeats) { _totalSeats = totalSeats; return this; }

    public Trip Build() =>
        Trip.Create(_id, _routeId, _origin, _destination, _departure, _price, _totalSeats);
}

namespace OnibusExpress.Tests.Fakes;

/// <summary>
/// In-memory store shared by the fake repositories. Reservations live INSIDE trips
/// (aggregate), so the Trip and Reservation repos read from the same place — a
/// reservation created through a trip is found by <c>GetByCodeAsync</c>.
/// </summary>
public sealed class InMemoryDatabase
{
    public List<Route> Routes { get; } = new();
    public List<Trip> Trips { get; } = new();

    public InMemoryDatabase WithRoute(Route route) { Routes.Add(route); return this; }
    public InMemoryDatabase WithTrip(Trip trip) { Trips.Add(trip); return this; }
}

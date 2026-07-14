namespace OnibusExpress.Tests.Application;

public class SearchTripsUseCaseTests
{
    [Fact]
    public async Task Execute_FiltersByOriginDestinationDate_DelegatesToRepo()
    {
        var tripA = new TripBuilder()
            .WithOrigin("Sao Paulo")
            .WithDestination("Rio de Janeiro")
            .WithDeparture(new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.Zero))
            .Build();
        var tripB = new TripBuilder()
            .WithOrigin("Curitiba")
            .WithDestination("Florianopolis")
            .WithDeparture(new DateTimeOffset(2030, 2, 2, 12, 0, 0, TimeSpan.Zero))
            .Build();

        var db = new InMemoryDatabase().WithTrip(tripA).WithTrip(tripB);
        var trips = new InMemoryTripRepository(db);
        var uc = new SearchTripsUseCase(trips);

        var query = new SearchTripsQuery("Sao Paulo", "Rio de Janeiro", new DateOnly(2030, 1, 1));

        var result = await uc.ExecuteAsync(query, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(tripA.Id, result[0].Id);
        Assert.Equal("Sao Paulo", result[0].Origin);
        Assert.Equal("Rio de Janeiro", result[0].Destination);
    }

    [Fact]
    public async Task Execute_MapsSummaryWithAvailableSeats()
    {
        var clock = new FakeClock(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var trip = new TripBuilder()
            .WithTotalSeats(44)
            .WithDeparture(clock.UtcNow.AddHours(3))
            .Build();

        var passenger = new PassengerBuilder().WithClock(clock).Build();
        trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12345"), clock);

        var db = new InMemoryDatabase().WithTrip(trip);
        var trips = new InMemoryTripRepository(db);
        var uc = new SearchTripsUseCase(trips);

        var query = new SearchTripsQuery(trip.Origin, trip.Destination, null);

        var result = await uc.ExecuteAsync(query, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(44, result[0].TotalSeats);
        Assert.Equal(43, result[0].AvailableSeats);
    }

    [Fact]
    public async Task Execute_NoResults_ReturnsEmptyList()
    {
        var trip = new TripBuilder()
            .WithOrigin("Sao Paulo")
            .WithDestination("Rio de Janeiro")
            .Build();

        var db = new InMemoryDatabase().WithTrip(trip);
        var trips = new InMemoryTripRepository(db);
        var uc = new SearchTripsUseCase(trips);

        var query = new SearchTripsQuery("Belo Horizonte", "Salvador", null);

        var result = await uc.ExecuteAsync(query, CancellationToken.None);

        Assert.Empty(result);
    }
}

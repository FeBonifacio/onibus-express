namespace OnibusExpress.Tests.Application;

public class GetTripUseCaseTests
{
    [Fact]
    public async Task Execute_TripWithReservations_ReturnsTakenAndFreeSeats()
    {
        var clock = new FakeClock(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).WithTotalSeats(10).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        trip.Reserve(passenger, SeatNumber.Create(2), ReservationCode.Create("ABC-12345"), clock);
        trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12346"), clock);

        var db = new InMemoryDatabase().WithTrip(trip);
        var trips = new InMemoryTripRepository(db);
        var uc = new GetTripUseCase(trips);

        var result = await uc.ExecuteAsync(trip.Id, CancellationToken.None);

        Assert.Equal(new[] { 2, 5 }, result.TakenSeats);
        Assert.Equal(new[] { 1, 3, 4, 6, 7, 8, 9, 10 }, result.FreeSeats);
        Assert.Equal(8, result.AvailableSeats);
    }

    [Fact]
    public async Task Execute_TripNotFound_ThrowsNotFound()
    {
        var db = new InMemoryDatabase();
        var trips = new InMemoryTripRepository(db);
        var uc = new GetTripUseCase(trips);

        await Assert.ThrowsAsync<NotFoundException>(() => uc.ExecuteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Execute_IgnoresCancelledReservations_InMap()
    {
        var clock = new FakeClock(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).WithTotalSeats(10).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        var first = trip.Reserve(passenger, SeatNumber.Create(2), ReservationCode.Create("ABC-12345"), clock);
        trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12346"), clock);

        first.Cancel(clock);

        var db = new InMemoryDatabase().WithTrip(trip);
        var trips = new InMemoryTripRepository(db);
        var uc = new GetTripUseCase(trips);

        var result = await uc.ExecuteAsync(trip.Id, CancellationToken.None);

        Assert.Equal(new[] { 5 }, result.TakenSeats);
        Assert.Equal(9, result.AvailableSeats);
    }
}

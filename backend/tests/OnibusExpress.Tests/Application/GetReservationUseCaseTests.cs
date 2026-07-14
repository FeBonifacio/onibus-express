namespace OnibusExpress.Tests.Application;

public class GetReservationUseCaseTests
{
    [Fact]
    public async Task Execute_ExistingCode_ReturnsMappedResponse()
    {
        var clock = new FakeClock(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var db = new InMemoryDatabase();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();
        var passenger = new PassengerBuilder().WithDocument("52998224725").WithClock(clock).Build();
        trip.Reserve(passenger, SeatNumber.Create(7), ReservationCode.Create("ABC-12345"), clock);
        db.WithTrip(trip);

        var reservations = new InMemoryReservationRepository(db);
        var uc = new GetReservationUseCase(reservations);

        var response = await uc.ExecuteAsync("ABC-12345", CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("ABC-12345", response.Code);
        Assert.Equal("529.982.247-25", response.DocumentFormatted);
        Assert.Equal("Active", response.Status);
        Assert.Equal(7, response.Seat);
    }

    [Fact]
    public async Task Execute_MalformedCode_ThrowsInvalidReservationCode()
    {
        var db = new InMemoryDatabase();
        var reservations = new InMemoryReservationRepository(db);
        var uc = new GetReservationUseCase(reservations);

        await Assert.ThrowsAsync<InvalidReservationCodeException>(
            () => uc.ExecuteAsync("xyz", CancellationToken.None));
    }

    [Fact]
    public async Task Execute_NotFound_ThrowsNotFound()
    {
        var db = new InMemoryDatabase();
        var reservations = new InMemoryReservationRepository(db);
        var uc = new GetReservationUseCase(reservations);

        await Assert.ThrowsAsync<NotFoundException>(
            () => uc.ExecuteAsync("ZZZ-99999", CancellationToken.None));
    }
}

namespace OnibusExpress.Tests.Application;

public class CancelReservationUseCaseTests
{
    private static FakeClock NewClock() =>
        new FakeClock(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static (Reservation reservation, CancelReservationUseCase uc, InMemoryReservationRepository reservations, NoopUnitOfWork uow)
        Setup(FakeClock clock, int departureHours)
    {
        var db = new InMemoryDatabase();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(departureHours)).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();
        var reservation = trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12345"), clock);
        db.WithTrip(trip);

        var reservations = new InMemoryReservationRepository(db);
        var uow = new NoopUnitOfWork();
        var uc = new CancelReservationUseCase(reservations, clock, uow);
        return (reservation, uc, reservations, uow);
    }

    [Fact]
    public async Task Execute_ReservationNotFound_ThrowsNotFound()
    {
        var clock = NewClock();
        var (_, uc, _, _) = Setup(clock, 3);

        await Assert.ThrowsAsync<NotFoundException>(() => uc.ExecuteAsync("ZZZ-99999", CancellationToken.None));
    }

    [Fact]
    public async Task Execute_MalformedCode_ThrowsInvalidReservationCode()
    {
        var clock = NewClock();
        var (_, uc, _, _) = Setup(clock, 3);

        await Assert.ThrowsAsync<InvalidReservationCodeException>(() => uc.ExecuteAsync("abc", CancellationToken.None));
    }

    [Fact]
    public async Task Execute_WithinWindow_CancelsAndPersists()
    {
        var clock = NewClock();
        var (reservation, uc, reservations, uow) = Setup(clock, 3);

        await uc.ExecuteAsync("ABC-12345", CancellationToken.None);

        Assert.Equal(1, reservations.UpdateCalls);
        Assert.Equal(1, uow.Commits);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public async Task Execute_TooLate_PropagatesTooLate()
    {
        var clock = NewClock();
        var (_, uc, _, _) = Setup(clock, 1);

        await Assert.ThrowsAsync<CancellationTooLateException>(() => uc.ExecuteAsync("ABC-12345", CancellationToken.None));
    }

    [Fact]
    public async Task Execute_AlreadyCancelled_PropagatesAlreadyCancelled()
    {
        var clock = NewClock();
        var (reservation, uc, _, _) = Setup(clock, 3);
        reservation.Cancel(clock);

        await Assert.ThrowsAsync<ReservationAlreadyCancelledException>(() => uc.ExecuteAsync("ABC-12345", CancellationToken.None));
    }
}

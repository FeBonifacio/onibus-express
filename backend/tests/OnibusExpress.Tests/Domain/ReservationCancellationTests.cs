namespace OnibusExpress.Tests.Domain;

public class ReservationCancellationTests
{
    private static (FakeClock clock, Reservation reservation) CreateActiveReservation(TimeSpan offset)
    {
        var clock = new FakeClock(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.Add(offset)).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();
        var reservation = trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12345"), clock);
        return (clock, reservation);
    }

    [Fact]
    public void Cancel_MoreThan2hBefore_SetsStatusCancelled()
    {
        var (clock, reservation) = CreateActiveReservation(TimeSpan.FromHours(3));
        reservation.Cancel(clock);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Cancel_Exactly2hBefore_ThrowsTooLate()
    {
        var (clock, reservation) = CreateActiveReservation(TimeSpan.FromHours(2));
        Assert.Throws<CancellationTooLateException>(() => reservation.Cancel(clock));
    }

    [Fact]
    public void Cancel_OneSecondAbove2h_Cancels()
    {
        var (clock, reservation) = CreateActiveReservation(TimeSpan.FromHours(2) + TimeSpan.FromSeconds(1));
        reservation.Cancel(clock);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Cancel_LessThan2hBefore_ThrowsTooLate()
    {
        var (clock, reservation) = CreateActiveReservation(TimeSpan.FromMinutes(90));
        Assert.Throws<CancellationTooLateException>(() => reservation.Cancel(clock));
    }

    [Fact]
    public void Cancel_AfterDeparture_ThrowsTooLate()
    {
        var (clock, reservation) = CreateActiveReservation(TimeSpan.FromHours(3));
        clock.Advance(TimeSpan.FromHours(4));
        Assert.Throws<CancellationTooLateException>(() => reservation.Cancel(clock));
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ThrowsAlreadyCancelled()
    {
        var (clock, reservation) = CreateActiveReservation(TimeSpan.FromHours(3));
        reservation.Cancel(clock);
        Assert.Throws<ReservationAlreadyCancelledException>(() => reservation.Cancel(clock));
    }

    [Theory]
    [InlineData(180, true)]
    [InlineData(120, false)]
    [InlineData(90, false)]
    public void CanCancel_ReturnsBoolWithoutThrowing(int offsetMinutes, bool expected)
    {
        var (clock, reservation) = CreateActiveReservation(TimeSpan.FromMinutes(offsetMinutes));
        Assert.Equal(expected, reservation.CanCancel(clock));
    }
}

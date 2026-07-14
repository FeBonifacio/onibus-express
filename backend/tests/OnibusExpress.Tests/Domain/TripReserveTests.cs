namespace OnibusExpress.Tests.Domain;

public class TripReserveTests
{
    private static FakeClock NewClock() =>
        new FakeClock(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Reserve_FreeSeat_CreatesActiveReservation()
    {
        var clock = NewClock();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).WithTotalSeats(44).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        var before = trip.AvailableSeats;
        var reservation = trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12345"), clock);

        Assert.True(reservation.IsActive);
        Assert.Contains(5, trip.TakenSeats());
        Assert.Equal(before - 1, trip.AvailableSeats);
    }

    [Fact]
    public void Reserve_SeatAlreadyTaken_ThrowsSeatTaken()
    {
        var clock = NewClock();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12345"), clock);

        Assert.Throws<SeatTakenException>(() =>
            trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-54321"), clock));
    }

    [Fact]
    public void Reserve_SeatCancelledBefore_AllowsReuse()
    {
        var clock = NewClock();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        var reservation = trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12345"), clock);
        reservation.Cancel(clock);

        var reused = trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-54321"), clock);

        Assert.True(reused.IsActive);
        Assert.Contains(5, trip.TakenSeats());
    }

    [Fact]
    public void Reserve_SeatAboveTotal_ThrowsSeatOutOfRange()
    {
        var clock = NewClock();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).WithTotalSeats(44).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        Assert.Throws<SeatOutOfRangeException>(() =>
            trip.Reserve(passenger, SeatNumber.Create(45), ReservationCode.Create("ABC-12345"), clock));
    }

    [Fact]
    public void Reserve_SeatEqualToTotal_Ok()
    {
        var clock = NewClock();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).WithTotalSeats(44).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        var reservation = trip.Reserve(passenger, SeatNumber.Create(44), ReservationCode.Create("ABC-12345"), clock);

        Assert.True(reservation.IsActive);
        Assert.Contains(44, trip.TakenSeats());
    }

    [Fact]
    public void Reserve_TripInPast_ThrowsTripAlreadyDeparted()
    {
        var clock = NewClock();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(-1)).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        Assert.Throws<TripAlreadyDepartedException>(() =>
            trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12345"), clock));
    }

    [Fact]
    public void Reserve_DepartureExactlyNow_ThrowsTripAlreadyDeparted()
    {
        var clock = NewClock();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        Assert.Throws<TripAlreadyDepartedException>(() =>
            trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12345"), clock));
    }

    [Fact]
    public void Reserve_CheckOrder_DepartedBeforeTaken()
    {
        var clock = NewClock();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(1)).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-12345"), clock);

        clock.Advance(TimeSpan.FromHours(2));

        Assert.Throws<TripAlreadyDepartedException>(() =>
            trip.Reserve(passenger, SeatNumber.Create(5), ReservationCode.Create("ABC-54321"), clock));
    }

    [Fact]
    public void FreeSeats_ReflectsOccupancy()
    {
        var clock = NewClock();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).WithTotalSeats(5).Build();
        var passenger = new PassengerBuilder().WithClock(clock).Build();

        trip.Reserve(passenger, SeatNumber.Create(2), ReservationCode.Create("ABC-12345"), clock);
        trip.Reserve(passenger, SeatNumber.Create(4), ReservationCode.Create("ABC-54321"), clock);

        var free = trip.FreeSeats();

        Assert.Equal(new[] { 1, 3, 5 }, free);
        Assert.DoesNotContain(2, free);
        Assert.DoesNotContain(4, free);
    }
}

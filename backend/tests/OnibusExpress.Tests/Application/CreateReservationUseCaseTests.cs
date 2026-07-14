namespace OnibusExpress.Tests.Application;

public class CreateReservationUseCaseTests
{
    private static FakeClock NewClock() =>
        new FakeClock(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Execute_ValidData_CreatesReservationAndPersists()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder()
            .WithPrice(199.90m)
            .WithDeparture(clock.UtcNow.AddHours(3))
            .Build();
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        var gen = new FakeReservationCodeGenerator("ABC-12345");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(trip.Id, "Maria", "52998224725", "maria@x.com", new DateOnly(1990, 5, 20), 5);

        var response = await uc.ExecuteAsync(req, CancellationToken.None);

        Assert.Equal("ABC-12345", response.Code);
        Assert.Equal(5, response.Seat);
        Assert.Equal("Active", response.Status);
        Assert.Equal(199.90m, response.Price);
        Assert.Equal(trip.DepartureUtc, response.DepartureUtc);
        Assert.Equal(1, trips.UpdateCalls);
        Assert.Equal(1, uow.Commits);

        var lookup = new InMemoryReservationRepository(db);
        var stored = await lookup.GetByCodeAsync(ReservationCode.Create("ABC-12345"), CancellationToken.None);
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task Execute_TripNotFound_ThrowsNotFound()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        var gen = new FakeReservationCodeGenerator("ABC-12345");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(Guid.NewGuid(), "Maria", "52998224725", "maria@x.com", new DateOnly(1990, 5, 20), 5);

        await Assert.ThrowsAsync<NotFoundException>(() => uc.ExecuteAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task Execute_InvalidDocument_ThrowsInvalidDocument()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        var gen = new FakeReservationCodeGenerator("ABC-12345");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(trip.Id, "Maria", "111.111.111-11", "maria@x.com", new DateOnly(1990, 5, 20), 5);

        await Assert.ThrowsAsync<InvalidDocumentException>(() => uc.ExecuteAsync(req, CancellationToken.None));
        Assert.Equal(0, trips.UpdateCalls);
    }

    [Fact]
    public async Task Execute_InvalidEmail_ThrowsInvalidEmail()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        var gen = new FakeReservationCodeGenerator("ABC-12345");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(trip.Id, "Maria", "52998224725", "invalid", new DateOnly(1990, 5, 20), 5);

        await Assert.ThrowsAsync<InvalidEmailException>(() => uc.ExecuteAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task Execute_SeatLessThanOne_ThrowsInvalidSeat()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        var gen = new FakeReservationCodeGenerator("ABC-12345");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(trip.Id, "Maria", "52998224725", "maria@x.com", new DateOnly(1990, 5, 20), 0);

        await Assert.ThrowsAsync<InvalidSeatException>(() => uc.ExecuteAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task Execute_SeatTaken_PropagatesSeatTaken()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();

        var existingPassenger = new PassengerBuilder().WithClock(clock).Build();
        trip.Reserve(existingPassenger, SeatNumber.Create(5), ReservationCode.Create("ZZZ-99999"), clock);
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        var gen = new FakeReservationCodeGenerator("ABC-12345");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(trip.Id, "Maria", "52998224725", "maria@x.com", new DateOnly(1990, 5, 20), 5);

        await Assert.ThrowsAsync<SeatTakenException>(() => uc.ExecuteAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task Execute_SeatAboveTotal_PropagatesSeatOutOfRange()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder()
            .WithTotalSeats(44)
            .WithDeparture(clock.UtcNow.AddHours(3))
            .Build();
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        var gen = new FakeReservationCodeGenerator("ABC-12345");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(trip.Id, "Maria", "52998224725", "maria@x.com", new DateOnly(1990, 5, 20), 45);

        await Assert.ThrowsAsync<SeatOutOfRangeException>(() => uc.ExecuteAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task Execute_TripDeparted_PropagatesTripAlreadyDeparted()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder()
            .WithDeparture(clock.UtcNow.AddHours(-1))
            .Build();
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        var gen = new FakeReservationCodeGenerator("ABC-12345");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(trip.Id, "Maria", "52998224725", "maria@x.com", new DateOnly(1990, 5, 20), 5);

        await Assert.ThrowsAsync<TripAlreadyDepartedException>(() => uc.ExecuteAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task Execute_CodeCollidesThenUnique_ReturnsUniqueCode()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        reservations.ForceCollisions(3);
        var gen = new FakeReservationCodeGenerator("AAA-11111", "BBB-22222", "CCC-33333", "DDD-44444");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(trip.Id, "Maria", "52998224725", "maria@x.com", new DateOnly(1990, 5, 20), 5);

        var response = await uc.ExecuteAsync(req, CancellationToken.None);

        Assert.Equal("DDD-44444", response.Code);
        Assert.Equal(4, gen.Calls);
    }

    [Fact]
    public async Task Execute_CodeAlwaysCollides_ThrowsCouldNotGenerateCode()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        reservations.ForceCollisions(5);
        var gen = new FakeReservationCodeGenerator("AAA-11111", "BBB-22222", "CCC-33333", "DDD-44444", "EEE-55555");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(trip.Id, "Maria", "52998224725", "maria@x.com", new DateOnly(1990, 5, 20), 5);

        await Assert.ThrowsAsync<CouldNotGenerateCodeException>(() => uc.ExecuteAsync(req, CancellationToken.None));
        Assert.Equal(0, uow.Commits);
        Assert.Equal(0, trips.UpdateCalls);
    }

    [Fact]
    public async Task Execute_CodeAlreadyPersisted_RetriesWithoutForceCollisions()
    {
        var clock = NewClock();
        var db = new InMemoryDatabase();
        var trip = new TripBuilder().WithDeparture(clock.UtcNow.AddHours(3)).Build();

        var existingPassenger = new PassengerBuilder().WithClock(clock).Build();
        trip.Reserve(existingPassenger, SeatNumber.Create(10), ReservationCode.Create("ABC-11111"), clock);
        db.WithTrip(trip);

        var trips = new InMemoryTripRepository(db);
        var reservations = new InMemoryReservationRepository(db);
        var gen = new FakeReservationCodeGenerator("ABC-11111", "ABC-22222");
        var uow = new NoopUnitOfWork();
        var uc = new CreateReservationUseCase(trips, reservations, gen, clock, uow);
        var req = new CreateReservationRequest(trip.Id, "Maria", "52998224725", "maria@x.com", new DateOnly(1990, 5, 20), 5);

        var response = await uc.ExecuteAsync(req, CancellationToken.None);

        Assert.Equal("ABC-22222", response.Code);
        Assert.Equal(2, gen.Calls);
    }
}

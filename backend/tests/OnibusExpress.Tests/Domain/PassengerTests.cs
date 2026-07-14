namespace OnibusExpress.Tests.Domain;

public class PassengerTests
{
    private static readonly DateTimeOffset Today = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_Valid_Ok()
    {
        var clock = new FakeClock(Today);
        var id = Guid.NewGuid();
        var document = Document.Create("52998224725");
        var email = Email.Create("maria@x.com");
        var birthDate = new DateOnly(1990, 5, 20);

        var passenger = Passenger.Create(id, "Maria", document, email, birthDate, clock);

        Assert.Equal("Maria", passenger.Name);
        Assert.Equal(document, passenger.Document);
        Assert.Equal(email, passenger.Email);
        Assert.Equal(birthDate, passenger.BirthDate);
    }

    [Fact]
    public void Create_EmptyName_ThrowsArgument()
    {
        var clock = new FakeClock(Today);
        var document = Document.Create("52998224725");
        var email = Email.Create("maria@x.com");
        var birthDate = new DateOnly(1990, 5, 20);

        Assert.Throws<ArgumentException>(() =>
            Passenger.Create(Guid.NewGuid(), "  ", document, email, birthDate, clock));
    }

    [Fact]
    public void Create_FutureBirthDate_ThrowsArgument()
    {
        var clock = new FakeClock(Today);
        var document = Document.Create("52998224725");
        var email = Email.Create("maria@x.com");
        var futureBirthDate = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime).AddDays(1);

        Assert.Throws<ArgumentException>(() =>
            Passenger.Create(Guid.NewGuid(), "Maria", document, email, futureBirthDate, clock));
    }

    [Fact]
    public void Create_BirthDateToday_Ok()
    {
        var clock = new FakeClock(Today);
        var document = Document.Create("52998224725");
        var email = Email.Create("maria@x.com");
        var birthDateToday = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

        var passenger = Passenger.Create(Guid.NewGuid(), "Maria", document, email, birthDateToday, clock);

        Assert.Equal(birthDateToday, passenger.BirthDate);
    }
}

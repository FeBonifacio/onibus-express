namespace OnibusExpress.Tests.Builders;

/// <summary>Fluent <see cref="Passenger"/> builder with a valid document by default.</summary>
public sealed class PassengerBuilder
{
    private string _name = "Maria Silva";
    private string _document = "52998224725";
    private string _email = "maria@exemplo.com";
    private DateOnly _birthDate = new(1990, 5, 20);
    private IClock _clock = new FakeClock(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));

    public PassengerBuilder WithName(string name) { _name = name; return this; }
    public PassengerBuilder WithDocument(string document) { _document = document; return this; }
    public PassengerBuilder WithEmail(string email) { _email = email; return this; }
    public PassengerBuilder WithBirthDate(DateOnly birthDate) { _birthDate = birthDate; return this; }
    public PassengerBuilder WithClock(IClock clock) { _clock = clock; return this; }

    public Passenger Build() =>
        Passenger.Create(Guid.NewGuid(), _name, Document.Create(_document), Email.Create(_email), _birthDate, _clock);
}

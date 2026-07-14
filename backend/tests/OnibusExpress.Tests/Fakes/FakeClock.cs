namespace OnibusExpress.Tests.Fakes;

/// <summary>Controllable clock to make time rules deterministic. Always UTC.</summary>
public sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; private set; }

    public FakeClock(DateTimeOffset start) => UtcNow = start.ToUniversalTime();

    public void Advance(TimeSpan delta) => UtcNow = UtcNow.Add(delta);

    public void Set(DateTimeOffset instant) => UtcNow = instant.ToUniversalTime();
}

namespace OnibusExpress.Tests.Fakes;

/// <summary>
/// Deterministic generator: dequeues seeded codes; once the queue is empty it produces
/// seeded pseudo-random codes (reproducible). Counts calls. Collisions are simulated in
/// the repository (see <see cref="InMemoryReservationRepository.ForceCollisions"/>).
/// </summary>
public sealed class FakeReservationCodeGenerator : IReservationCodeGenerator
{
    private readonly Queue<ReservationCode> _queue = new();
    private readonly Random _rng = new(12345);

    public int Calls { get; private set; }

    public FakeReservationCodeGenerator(params string[] codes)
    {
        foreach (var c in codes)
        {
            _queue.Enqueue(ReservationCode.Create(c));
        }
    }

    public FakeReservationCodeGenerator Enqueue(string code)
    {
        _queue.Enqueue(ReservationCode.Create(code));
        return this;
    }

    public ReservationCode Generate()
    {
        Calls++;
        return _queue.Count > 0 ? _queue.Dequeue() : ReservationCode.NewRandom(_rng.Next);
    }
}

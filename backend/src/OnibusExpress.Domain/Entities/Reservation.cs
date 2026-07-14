namespace OnibusExpress.Domain.Entities;

public sealed class Reservation : Entity
{
    public static readonly TimeSpan CancellationWindow = TimeSpan.FromHours(2);

    public Guid TripId { get; private init; }
    public Passenger Passenger { get; private init; } = default!;
    public SeatNumber Seat { get; private init; } = default!;
    public ReservationCode Code { get; private set; } = default!;
    public ReservationStatus Status { get; private set; }

    /// <summary>Snapshot of the departure (UTC): lets us apply the 2h rule without loading the Trip.</summary>
    public DateTimeOffset DepartureUtc { get; private init; }

    /// <summary>Snapshot of the price at booking time (lets us read the reservation without the Trip).</summary>
    public decimal Price { get; private init; }

    public bool IsActive => Status == ReservationStatus.Active;

    private Reservation() { } // hydration (Phase 3 / EF)

    internal static Reservation Create(
        Guid tripId, DateTimeOffset departureUtc, decimal price, Passenger passenger,
        SeatNumber seat, ReservationCode code) =>
        new()
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            DepartureUtc = departureUtc,
            Price = price,
            Passenger = passenger,
            Seat = seat,
            Code = code,
            Status = ReservationStatus.Active,
        };

    /// <summary>STRICT boundary: with EXACTLY 2h (or less) left before departure, cannot cancel.</summary>
    public bool CanCancel(IClock clock) =>
        Status == ReservationStatus.Active && (DepartureUtc - clock.UtcNow) > CancellationWindow;

    public void Cancel(IClock clock)
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new ReservationAlreadyCancelledException(Code.Value);
        }

        if (!CanCancel(clock))
        {
            throw new CancellationTooLateException(Code.Value, DepartureUtc);
        }

        Status = ReservationStatus.Cancelled;
    }

    /// <summary>Reserved for Phase 3 (retry after a unique-index collision). Unused in Phase 2.</summary>
    internal void RegenerateCode(ReservationCode code) => Code = code;
}

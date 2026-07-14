namespace OnibusExpress.Domain.Entities;

/// <summary>
/// Aggregate Root. Owns its <see cref="Reservation"/>s and protects business rules
/// 1 (seat already taken) and 2 (trip already departed) inside <see cref="Reserve"/>.
/// </summary>
public sealed class Trip : Entity
{
    private readonly List<Reservation> _reservations = new();

    public Guid RouteId { get; private init; }
    public string Origin { get; private init; } = default!;      // denormalized for search
    public string Destination { get; private init; } = default!;
    public DateTimeOffset DepartureUtc { get; private init; }
    public decimal BasePrice { get; private init; }
    public int TotalSeats { get; private init; }

    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private Trip() { } // hydration (Phase 3 / EF)

    public static Trip Create(
        Guid id, Guid routeId, string origin, string destination,
        DateTimeOffset departureUtc, decimal basePrice, int totalSeats)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new ArgumentException("Origem obrigatoria.", nameof(origin));
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentException("Destino obrigatorio.", nameof(destination));
        }

        if (totalSeats <= 0)
        {
            throw new ArgumentException("Total de assentos deve ser positivo.", nameof(totalSeats));
        }

        if (basePrice < 0)
        {
            throw new ArgumentException("Preco base nao pode ser negativo.", nameof(basePrice));
        }

        return new Trip
        {
            Id = id,
            RouteId = routeId,
            Origin = origin.Trim(),
            Destination = destination.Trim(),
            DepartureUtc = departureUtc.ToUniversalTime(),
            BasePrice = basePrice,
            TotalSeats = totalSeats,
        };
    }

    /// <summary>Rules 1 and 2 live here. Order: departed -> out of range -> taken.</summary>
    public Reservation Reserve(Passenger passenger, SeatNumber seat, ReservationCode code, IClock clock)
    {
        if (HasDeparted(clock))
        {
            throw new TripAlreadyDepartedException(Id, DepartureUtc);
        }

        if (seat.Value < 1 || seat.Value > TotalSeats)
        {
            throw new SeatOutOfRangeException(seat.Value, TotalSeats);
        }

        if (IsSeatTaken(seat))
        {
            throw new SeatTakenException(Id, seat.Value);
        }

        var reservation = Reservation.Create(Id, DepartureUtc, BasePrice, passenger, seat, code);
        _reservations.Add(reservation);
        return reservation;
    }

    /// <summary>Departed AT THE EXACT departure instant (&lt;=): from then on, no more reservations.</summary>
    public bool HasDeparted(IClock clock) => DepartureUtc <= clock.UtcNow;

    public bool IsSeatTaken(SeatNumber seat) =>
        _reservations.Any(r => r.IsActive && r.Seat == seat);

    public int AvailableSeats => TotalSeats - _reservations.Count(r => r.IsActive);

    public IReadOnlyList<int> TakenSeats() =>
        _reservations.Where(r => r.IsActive).Select(r => r.Seat.Value).OrderBy(n => n).ToList();

    public IReadOnlyList<int> FreeSeats() =>
        Enumerable.Range(1, TotalSeats).Except(TakenSeats()).ToList();

    internal void LoadReservations(IEnumerable<Reservation> reservations) // hydration (Phase 3 / EF)
    {
        _reservations.Clear();
        _reservations.AddRange(reservations);
    }
}

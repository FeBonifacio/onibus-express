namespace OnibusExpress.Domain.Exceptions;

public sealed class SeatTakenException : DomainException
{
    public override string ErrorCode => "SEAT_TAKEN";
    public Guid TripId { get; }
    public int Seat { get; }

    public SeatTakenException(Guid tripId, int seat)
        : base($"Assento {seat} ja esta ocupado nesta viagem.")
    {
        TripId = tripId;
        Seat = seat;
    }

    /// <summary>For the persistence-level unique-index conflict (concurrent booking), when the seat context is not at hand.</summary>
    public SeatTakenException()
        : base("Assento ja esta ocupado nesta viagem.") { }
}

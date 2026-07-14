namespace OnibusExpress.Domain.Exceptions;

public sealed class TripAlreadyDepartedException : DomainException
{
    public override string ErrorCode => "TRIP_ALREADY_DEPARTED";
    public Guid TripId { get; }
    public DateTimeOffset Departure { get; }

    public TripAlreadyDepartedException(Guid tripId, DateTimeOffset departure)
        : base($"Viagem {tripId} ja partiu ({departure:u}); nao e possivel reservar.")
    {
        TripId = tripId;
        Departure = departure;
    }
}

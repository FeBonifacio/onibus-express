namespace OnibusExpress.Domain.Exceptions;

public sealed class ReservationAlreadyCancelledException : DomainException
{
    public override string ErrorCode => "RESERVATION_ALREADY_CANCELLED";
    public string Code { get; }

    public ReservationAlreadyCancelledException(string code)
        : base($"Reserva {code} ja esta cancelada.") => Code = code;
}

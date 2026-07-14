namespace OnibusExpress.Domain.Exceptions;

public sealed class CancellationTooLateException : DomainException
{
    public override string ErrorCode => "CANCELLATION_TOO_LATE";
    public string Code { get; }
    public DateTimeOffset Departure { get; }

    public CancellationTooLateException(string code, DateTimeOffset departure)
        : base($"Cancelamento da reserva {code} nao permitido: prazo e ate 2h antes da partida ({departure:u}).")
    {
        Code = code;
        Departure = departure;
    }
}

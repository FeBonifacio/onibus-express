namespace OnibusExpress.Domain.Exceptions;

public sealed class InvalidReservationCodeException : DomainException
{
    public override string ErrorCode => "RESERVATION_CODE_INVALID";
    public string Value { get; }

    public InvalidReservationCodeException(string value)
        : base($"Codigo de reserva invalido: '{value}'. Formato esperado: ABC-12345.") => Value = value;
}

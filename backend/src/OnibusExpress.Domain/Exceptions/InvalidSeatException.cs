namespace OnibusExpress.Domain.Exceptions;

public sealed class InvalidSeatException : DomainException
{
    public override string ErrorCode => "SEAT_INVALID";
    public int Value { get; }

    public InvalidSeatException(int value)
        : base($"Numero de assento invalido: {value}. Deve ser >= 1.") => Value = value;
}

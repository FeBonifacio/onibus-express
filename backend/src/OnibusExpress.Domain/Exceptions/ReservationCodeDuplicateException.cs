namespace OnibusExpress.Domain.Exceptions;

/// <summary>
/// Raised when the persistence layer rejects a duplicate reservation code (unique-index
/// violation on a concurrent insert). Maps to HTTP 409.
/// </summary>
public sealed class ReservationCodeDuplicateException : DomainException
{
    public override string ErrorCode => "RESERVATION_CODE_DUPLICATE";

    public ReservationCodeDuplicateException()
        : base("Codigo de reserva duplicado; tente novamente.") { }
}

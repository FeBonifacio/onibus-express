namespace OnibusExpress.Application.Abstractions;

/// <summary>Produces a well-formed reservation code candidate. Uniqueness is the use case's job.</summary>
public interface IReservationCodeGenerator
{
    ReservationCode Generate();
}

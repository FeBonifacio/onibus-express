namespace OnibusExpress.Domain.Exceptions;

public sealed class SeatOutOfRangeException : DomainException
{
    public override string ErrorCode => "SEAT_OUT_OF_RANGE";
    public int Seat { get; }
    public int TotalSeats { get; }

    public SeatOutOfRangeException(int seat, int totalSeats)
        : base($"Assento {seat} nao existe nesta viagem (total: {totalSeats}).")
    {
        Seat = seat;
        TotalSeats = totalSeats;
    }
}

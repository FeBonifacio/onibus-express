namespace OnibusExpress.Domain.ValueObjects;

/// <summary>
/// Seat number (>= 1). A sealed class (not a struct) so the invariant cannot be
/// bypassed via <c>default</c>. The upper bound (&gt; trip total) is enforced by
/// <see cref="Entities.Trip.Reserve"/>, which is the only type that knows the total.
/// </summary>
public sealed class SeatNumber : IEquatable<SeatNumber>
{
    public int Value { get; }

    private SeatNumber(int value) => Value = value;

    public static SeatNumber Create(int value)
    {
        if (value < 1)
        {
            throw new InvalidSeatException(value);
        }

        return new SeatNumber(value);
    }

    public bool Equals(SeatNumber? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as SeatNumber);
    public override int GetHashCode() => Value;

    public static bool operator ==(SeatNumber? left, SeatNumber? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(SeatNumber? left, SeatNumber? right) => !(left == right);

    public override string ToString() => Value.ToString();
}

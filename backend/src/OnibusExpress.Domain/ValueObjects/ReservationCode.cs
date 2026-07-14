namespace OnibusExpress.Domain.ValueObjects;

/// <summary>
/// Human-readable reservation code in the form <c>ABC-12345</c>. A sealed class
/// (not a struct) so there is no bypassable <c>default</c> with a null value.
/// The value object guarantees the SHAPE and randomness; UNIQUENESS is the
/// Application's responsibility (Domain does not know about persistence), which is
/// why <see cref="NewRandom"/> takes the randomness source as a delegate.
/// </summary>
public sealed class ReservationCode : IEquatable<ReservationCode>
{
    private static readonly Regex Pattern = new("^[A-Z]{3}-[0-9]{5}$", RegexOptions.Compiled);
    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public string Value { get; }

    private ReservationCode(string value) => Value = value;

    /// <summary>Parses/validates an existing code (e.g. GET /reservas/{code}).</summary>
    public static ReservationCode Create(string? value)
    {
        var v = (value ?? string.Empty).Trim().ToUpperInvariant();
        if (!Pattern.IsMatch(v))
        {
            throw new InvalidReservationCodeException(value ?? string.Empty);
        }

        return new ReservationCode(v);
    }

    /// <summary>
    /// Builds a well-formed candidate. <paramref name="next"/> must return an int in
    /// [0, max). Uniqueness is verified by the caller.
    /// </summary>
    public static ReservationCode NewRandom(Func<int, int> next)
    {
        Span<char> buffer = stackalloc char[9];
        for (int i = 0; i < 3; i++)
        {
            buffer[i] = Letters[next(26)];
        }

        buffer[3] = '-';
        for (int i = 0; i < 5; i++)
        {
            buffer[4 + i] = (char)('0' + next(10));
        }

        return new ReservationCode(new string(buffer));
    }

    public bool Equals(ReservationCode? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as ReservationCode);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ReservationCode? left, ReservationCode? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(ReservationCode? left, ReservationCode? right) => !(left == right);

    public override string ToString() => Value;
}

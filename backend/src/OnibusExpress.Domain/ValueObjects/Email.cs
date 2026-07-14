namespace OnibusExpress.Domain.ValueObjects;

/// <summary>Normalized (trimmed) e-mail, with case-insensitive equality.</summary>
public sealed class Email : IEquatable<Email>
{
    private static readonly Regex Pattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string? input)
    {
        var v = (input ?? string.Empty).Trim();
        if (v.Length == 0 || !Pattern.IsMatch(v))
        {
            throw new InvalidEmailException(input ?? string.Empty);
        }

        return new Email(v);
    }

    public bool Equals(Email? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => Equals(obj as Email);

    // Must use the SAME comparer as Equals, otherwise equal instances could hash differently.
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString() => Value;
}

namespace OnibusExpress.Domain.ValueObjects;

/// <summary>
/// Passenger document. Named generically so it can scale to other document types;
/// today it validates CPF (check digit, module 11) as required. Immutable, value
/// equality. Lenient parsing: strips non-ASCII-digit characters before validating.
/// </summary>
public sealed class Document : IEquatable<Document>
{
    /// <summary>Normalized digits, no mask.</summary>
    public string Value { get; }
    public DocumentType Type { get; }

    private Document(string value, DocumentType type)
    {
        Value = value;
        Type = type;
    }

    public static Document Create(string? input, DocumentType type = DocumentType.Cpf)
    {
        var digits = DigitsOnly(input);
        if (!IsValidFor(digits, type))
        {
            throw new InvalidDocumentException(input ?? string.Empty, type);
        }

        return new Document(digits, type);
    }

    /// <summary>Predicate that does not throw, for frequent checks.</summary>
    public static bool IsValid(string? input, DocumentType type = DocumentType.Cpf) =>
        IsValidFor(DigitsOnly(input), type);

    public string Formatted => Type switch
    {
        DocumentType.Cpf => $"{Value[..3]}.{Value.Substring(3, 3)}.{Value.Substring(6, 3)}-{Value[9..]}",
        _ => Value,
    };

    private static bool IsValidFor(string digits, DocumentType type) => type switch
    {
        DocumentType.Cpf => IsValidCpf(digits),
        _ => false,
    };

    // --- CPF (module 11 check digits) ---
    private static bool IsValidCpf(string d)
    {
        if (d.Length != 11)
        {
            return false;          // step 1: length
        }

        if (AllSameDigits(d))
        {
            return false;        // step 2: repeated sequence
        }

        var s = d.AsSpan();
        if (CpfCheckDigit(s, 9, 10) != d[9] - '0')
        {
            return false;    // step 3: 1st check digit
        }

        return CpfCheckDigit(s, 10, 11) == d[10] - '0';             // step 4: 2nd check digit
    }

    private static int CpfCheckDigit(ReadOnlySpan<char> d, int count, int startWeight)
    {
        int sum = 0, weight = startWeight;
        for (int i = 0; i < count; i++)
        {
            sum += (d[i] - '0') * weight--;
        }

        int rest = sum % 11;
        return rest < 2 ? 0 : 11 - rest;
    }

    private static bool AllSameDigits(string d)
    {
        for (int i = 1; i < d.Length; i++)
        {
            if (d[i] != d[0])
            {
                return false;
            }
        }

        return true;
    }

    // ASCII digits only: char.IsDigit would accept non-ASCII Unicode Nd digits,
    // corrupting the (c - '0') arithmetic and the length gate.
    private static string DigitsOnly(string? s) =>
        new((s ?? string.Empty).Where(c => c is >= '0' and <= '9').ToArray());

    public bool Equals(Document? other) =>
        other is not null && Type == other.Type && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as Document);
    public override int GetHashCode() => HashCode.Combine(Type, Value);
    public override string ToString() => Formatted;
}

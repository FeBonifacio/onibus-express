namespace OnibusExpress.Domain.Entities;

public sealed class Passenger : Entity
{
    public string Name { get; private init; } = default!;
    public Document Document { get; private init; } = default!;
    public Email Email { get; private init; } = default!;
    public DateOnly BirthDate { get; private init; }

    private Passenger() { } // hydration (Phase 3 / EF)

    public static Passenger Create(Guid id, string name, Document document, Email email, DateOnly birthDate, IClock clock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Nome obrigatorio.", nameof(name));
        }

        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        if (birthDate > today)
        {
            throw new ArgumentException("Data de nascimento nao pode ser futura.", nameof(birthDate));
        }

        return new Passenger
        {
            Id = id,
            Name = name.Trim(),
            Document = document,
            Email = email,
            BirthDate = birthDate,
        };
    }
}

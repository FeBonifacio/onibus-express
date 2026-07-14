namespace OnibusExpress.Domain.Entities;

public sealed class Route : Entity
{
    public string Origin { get; private init; } = default!;
    public string Destination { get; private init; } = default!;
    public TimeSpan EstimatedDuration { get; private init; }

    private Route() { } // hydration (Phase 3 / EF)

    public static Route Create(Guid id, string origin, string destination, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new ArgumentException("Origem obrigatoria.", nameof(origin));
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentException("Destino obrigatorio.", nameof(destination));
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Duracao deve ser positiva.", nameof(duration));
        }

        return new Route
        {
            Id = id,
            Origin = origin.Trim(),
            Destination = destination.Trim(),
            EstimatedDuration = duration,
        };
    }
}

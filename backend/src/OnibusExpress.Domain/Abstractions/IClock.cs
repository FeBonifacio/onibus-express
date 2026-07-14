namespace OnibusExpress.Domain.Abstractions;

/// <summary>
/// Injectable clock. <see cref="UtcNow"/> is ALWAYS UTC, making the time rules
/// (departed trip, 2h cancellation window) deterministic and testable.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

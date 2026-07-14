namespace OnibusExpress.Application.Common;

/// <summary>Production <see cref="IClock"/> (system clock, UTC).</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

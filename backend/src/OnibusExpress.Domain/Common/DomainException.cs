namespace OnibusExpress.Domain.Common;

/// <summary>
/// Root of business-rule violations. Each concrete exception exposes a stable
/// <see cref="ErrorCode"/>, mapped to HTTP by the API layer (Phase 3).
/// Messages stay in Portuguese on purpose: they are user-facing feedback.
/// </summary>
public abstract class DomainException : Exception, IHasErrorCode
{
    public abstract string ErrorCode { get; }

    protected DomainException(string message) : base(message) { }
}

namespace OnibusExpress.Application.Common;

/// <summary>
/// Commits the changes of an operation. No-op in Phase 2 (in-memory repositories);
/// maps to <c>DbContext.SaveChangesAsync</c> in Phase 3.
/// </summary>
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken ct);
}

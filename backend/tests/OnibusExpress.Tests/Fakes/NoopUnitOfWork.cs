namespace OnibusExpress.Tests.Fakes;

public sealed class NoopUnitOfWork : IUnitOfWork
{
    public int Commits { get; private set; }

    public Task CommitAsync(CancellationToken ct)
    {
        Commits++;
        return Task.CompletedTask;
    }
}

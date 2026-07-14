namespace OnibusExpress.Tests.Fakes;

public sealed class InMemoryRouteRepository : IRouteRepository
{
    private readonly InMemoryDatabase _db;

    public InMemoryRouteRepository(InMemoryDatabase db) => _db = db;

    public Task<IReadOnlyList<Route>> ListAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Route>>(_db.Routes.ToList());
}

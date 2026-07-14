namespace OnibusExpress.Infrastructure.Repositories;

public sealed class EfRouteRepository : IRouteRepository
{
    private readonly AppDbContext _db;

    public EfRouteRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Route>> ListAsync(CancellationToken ct) =>
        await _db.Routes.AsNoTracking().OrderBy(r => r.Origin).ThenBy(r => r.Destination).ToListAsync(ct);
}

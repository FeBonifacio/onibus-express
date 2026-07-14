namespace OnibusExpress.Application.Abstractions;

public interface IRouteRepository
{
    Task<IReadOnlyList<Route>> ListAsync(CancellationToken ct);
}

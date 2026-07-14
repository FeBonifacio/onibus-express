namespace OnibusExpress.Application.UseCases.Routes;

/// <summary>GET /rotas — lists all available routes.</summary>
public sealed class ListRoutesUseCase
{
    private readonly IRouteRepository _routes;

    public ListRoutesUseCase(IRouteRepository routes) => _routes = routes;

    public async Task<IReadOnlyList<RouteResponse>> ExecuteAsync(CancellationToken ct)
    {
        var routes = await _routes.ListAsync(ct);
        return routes.Select(RouteMapper.ToResponse).ToList();
    }
}

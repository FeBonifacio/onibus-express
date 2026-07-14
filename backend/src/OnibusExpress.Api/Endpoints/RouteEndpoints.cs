namespace OnibusExpress.Api.Endpoints;

public static class RouteEndpoints
{
    public static void MapRouteEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/rotas", async (ListRoutesUseCase useCase, CancellationToken ct) =>
                Results.Ok(await useCase.ExecuteAsync(ct)))
            .WithTags("Rotas")
            .WithSummary("Lista todas as rotas disponiveis");
    }
}

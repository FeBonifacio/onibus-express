namespace OnibusExpress.Api.Endpoints;

public static class TripEndpoints
{
    public static void MapTripEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/viagens").WithTags("Viagens");

        group.MapGet("", async (string? origem, string? destino, DateOnly? data, SearchTripsUseCase useCase, CancellationToken ct) =>
                Results.Ok(await useCase.ExecuteAsync(new SearchTripsQuery(origem, destino, data), ct)))
            .WithSummary("Busca viagens por origem, destino e data");

        group.MapGet("/{id:guid}", async (Guid id, GetTripUseCase useCase, CancellationToken ct) =>
                Results.Ok(await useCase.ExecuteAsync(id, ct)))
            .WithSummary("Detalhe de uma viagem (assentos livres/ocupados)");
    }
}

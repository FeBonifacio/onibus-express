namespace OnibusExpress.Api.Endpoints;

public static class ReservationEndpoints
{
    public static void MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reservas").WithTags("Reservas");

        group.MapPost("", async (CreateReservationRequest request, CreateReservationUseCase useCase, CancellationToken ct) =>
            {
                var reservation = await useCase.ExecuteAsync(request, ct);
                return Results.Created($"/reservas/{reservation.Code}", reservation);
            })
            .WithSummary("Cria uma reserva (nome, documento, e-mail, viagem, assento)");

        group.MapGet("/{codigo}", async (string codigo, GetReservationUseCase useCase, CancellationToken ct) =>
                Results.Ok(await useCase.ExecuteAsync(codigo, ct)))
            .WithSummary("Consulta uma reserva pelo codigo gerado");

        group.MapDelete("/{codigo}", async (string codigo, CancelReservationUseCase useCase, CancellationToken ct) =>
            {
                await useCase.ExecuteAsync(codigo, ct);
                return Results.NoContent();
            })
            .WithSummary("Cancela uma reserva (ate 2h antes da partida)");
    }
}

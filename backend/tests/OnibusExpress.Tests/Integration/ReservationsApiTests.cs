using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OnibusExpress.Tests.Integration;

public sealed class ReservationsApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ReservationsApiTests(ApiFactory factory) => _factory = factory;

    private static object Body(Guid tripId, int seat, string document = "52998224725") => new
    {
        tripId,
        name = "Maria Silva",
        document,
        email = "maria@exemplo.com",
        birthDate = "1990-05-20",
        seat,
    };

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        var element = await response.Content.ReadFromJsonAsync<JsonElement>();
        return element.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    private static async Task<string> CreateReservationAsync(HttpClient client, Guid tripId, int seat)
    {
        var response = await client.PostAsJsonAsync("/reservas", Body(tripId, seat));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("code").GetString()!;
    }

    [Fact]
    public async Task Post_ValidReservation_Returns201WithReadableCode()
    {
        var tripId = await _factory.SeedTripAsync();
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/reservas", Body(tripId, 10));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Matches("^[A-Z]{3}-[0-9]{5}$", body.GetProperty("code").GetString()!);
        Assert.Equal("Active", body.GetProperty("status").GetString());
        Assert.Equal("529.982.247-25", body.GetProperty("documentFormatted").GetString());
    }

    [Fact]
    public async Task Post_DuplicateSeat_Returns409SeatTaken()
    {
        var tripId = await _factory.SeedTripAsync();
        var client = _factory.CreateClient();
        await CreateReservationAsync(client, tripId, 12);

        var response = await client.PostAsJsonAsync("/reservas", Body(tripId, 12));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("SEAT_TAKEN", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Post_InvalidDocument_Returns400()
    {
        var tripId = await _factory.SeedTripAsync();
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/reservas", Body(tripId, 5, "111.111.111-11"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("DOCUMENT_INVALID", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Post_DepartedTrip_Returns409TripAlreadyDeparted()
    {
        var tripId = await _factory.SeedTripAsync(untilDeparture: TimeSpan.FromHours(-1));
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/reservas", Body(tripId, 5));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("TRIP_ALREADY_DEPARTED", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Get_ByCode_ReturnsReservation()
    {
        var tripId = await _factory.SeedTripAsync();
        var client = _factory.CreateClient();
        var code = await CreateReservationAsync(client, tripId, 7);

        var response = await client.GetAsync($"/reservas/{code}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(7, body.GetProperty("seat").GetInt32());
        Assert.Equal(code, body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_UnknownCode_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/reservas/ZZZ-99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("NOT_FOUND", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Delete_WithinWindow_CancelsAndFreesSeat()
    {
        var tripId = await _factory.SeedTripAsync(untilDeparture: TimeSpan.FromDays(1));
        var client = _factory.CreateClient();
        var code = await CreateReservationAsync(client, tripId, 20);

        var delete = await client.DeleteAsync($"/reservas/{code}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var after = await (await client.GetAsync($"/reservas/{code}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Cancelled", after.GetProperty("status").GetString());

        // seat 20 is reusable after cancellation
        var reuse = await client.PostAsJsonAsync("/reservas", Body(tripId, 20));
        Assert.Equal(HttpStatusCode.Created, reuse.StatusCode);
    }

    [Fact]
    public async Task Delete_TooLate_Returns409()
    {
        var tripId = await _factory.SeedTripAsync(untilDeparture: TimeSpan.FromHours(1));
        var client = _factory.CreateClient();
        var code = await CreateReservationAsync(client, tripId, 3);

        var delete = await client.DeleteAsync($"/reservas/{code}");

        Assert.Equal(HttpStatusCode.Conflict, delete.StatusCode);
        Assert.Equal("CANCELLATION_TOO_LATE", await ErrorCodeAsync(delete));
    }
}

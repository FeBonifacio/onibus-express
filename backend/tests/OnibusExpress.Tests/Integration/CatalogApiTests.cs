using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OnibusExpress.Tests.Integration;

public sealed class CatalogApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public CatalogApiTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Health_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Rotas_ReturnsSeededRoutes()
    {
        await _factory.SeedTripAsync(); // also seeds a route
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/rotas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var routes = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(routes.GetArrayLength() >= 1);
        var first = routes.EnumerateArray().First();
        Assert.False(string.IsNullOrWhiteSpace(first.GetProperty("origin").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(first.GetProperty("destination").GetString()));
    }

    [Fact]
    public async Task Get_Viagens_FiltersByOriginAndDestination()
    {
        await _factory.SeedTripAsync();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/viagens?origem=Sao%20Paulo&destino=Rio%20de%20Janeiro");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var trips = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(trips.GetArrayLength() >= 1);
        // every returned trip actually matches the filter
        Assert.All(trips.EnumerateArray().ToList(), t =>
        {
            Assert.Equal("Sao Paulo", t.GetProperty("origin").GetString());
            Assert.Equal("Rio de Janeiro", t.GetProperty("destination").GetString());
        });

        // a non-seeded origin returns no results
        var none = await client.GetAsync("/viagens?origem=Manaus");
        var noneTrips = await none.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, noneTrips.GetArrayLength());
    }

    [Fact]
    public async Task Get_ViagemDetail_ShowsTakenAndFreeSeats()
    {
        var tripId = await _factory.SeedTripAsync(totalSeats: 10);
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/reservas", new
        {
            tripId,
            name = "Ana",
            document = "52998224725",
            email = "ana@exemplo.com",
            birthDate = "1990-01-01",
            seat = 4,
        });

        var response = await client.GetAsync($"/viagens/{tripId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(10, body.GetProperty("totalSeats").GetInt32());
        Assert.Equal(9, body.GetProperty("availableSeats").GetInt32());
        var taken = body.GetProperty("takenSeats").EnumerateArray().Select(e => e.GetInt32()).ToList();
        Assert.Contains(4, taken);
    }

    [Fact]
    public async Task Get_UnknownViagem_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/viagens/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

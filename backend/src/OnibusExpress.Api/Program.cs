using Microsoft.OpenApi.Models;
using OnibusExpress.Api.Common;
using OnibusExpress.Api.Endpoints;
using OnibusExpress.Api.Errors;
using OnibusExpress.Application;
using OnibusExpress.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Connection string from ConnectionStrings:Default. In Docker it is overridden by the
// standard ConnectionStrings__Default env var (see docker-compose.yml); a local dev default
// is the fallback.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=onibus;Username=onibus;Password=onibus";

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "OniBus Express API",
    Version = "v1",
    Description = "Sistema de venda de passagens rodoviarias (MVP).",
}));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
// Materializes framework-generated 4xx (malformed JSON, unparseable route/query) as ProblemDetails.
app.UseStatusCodePages();

// Apply migrations + seed on startup (skipped under the "Testing" environment, which
// configures its own SQLite schema).
if (!app.Environment.IsEnvironment("Testing"))
{
    await app.MigrateAndSeedAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithTags("Infra").WithSummary("Health check");
app.MapGet("/version", () => Results.Ok(new { version = AppVersion.Current }))
    .WithTags("Infra").WithSummary("Versao da API");

app.MapRouteEndpoints();
app.MapTripEndpoints();
app.MapReservationEndpoints();

app.Run();

// Exposed for WebApplicationFactory<Program> in the integration tests.
public partial class Program { }

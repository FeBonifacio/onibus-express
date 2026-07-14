using OnibusExpress.Infrastructure.Persistence;

namespace OnibusExpress.Api.Common;

public static class WebApplicationExtensions
{
    /// <summary>Applies migrations (PostgreSQL) or creates the schema (tests/SQLite), then seeds.</summary>
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        if (db.Database.IsNpgsql())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        await DbSeeder.SeedAsync(db, clock);
    }
}

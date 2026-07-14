namespace OnibusExpress.Infrastructure.Persistence;

/// <summary>
/// EF Core context. Maps the relational schema: Routes, Trips, Passengers, Reservations
/// (with foreign keys). Reservations are the children of the Trip aggregate but also have
/// their own DbSet for read/cancellation access.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

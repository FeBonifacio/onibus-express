namespace OnibusExpress.Infrastructure.Persistence.Configurations;

public sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> b)
    {
        b.ToTable("Trips");
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedNever(); // domain generates the Guid

        b.Property(t => t.Origin).IsRequired().HasMaxLength(120);
        b.Property(t => t.Destination).IsRequired().HasMaxLength(120);
        b.Property(t => t.DepartureUtc).IsRequired();
        b.Property(t => t.BasePrice).HasColumnType("numeric(10,2)");
        b.Property(t => t.TotalSeats).IsRequired();

        // Relational FK: Trip -> Route
        b.HasOne<Route>()
            .WithMany()
            .HasForeignKey(t => t.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Trip aggregate owns its Reservations, accessed via the _reservations backing field
        b.HasMany(t => t.Reservations)
            .WithOne()
            .HasForeignKey(r => r.TripId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(t => t.Reservations).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasIndex(t => new { t.Origin, t.Destination });
        b.HasIndex(t => t.DepartureUtc);
    }
}

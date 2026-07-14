namespace OnibusExpress.Infrastructure.Persistence.Configurations;

public sealed class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> b)
    {
        b.ToTable("Routes");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedNever(); // domain generates the Guid

        b.Property(r => r.Origin).IsRequired().HasMaxLength(120);
        b.Property(r => r.Destination).IsRequired().HasMaxLength(120);
        b.Property(r => r.EstimatedDuration).IsRequired();

        b.HasIndex(r => new { r.Origin, r.Destination });
    }
}

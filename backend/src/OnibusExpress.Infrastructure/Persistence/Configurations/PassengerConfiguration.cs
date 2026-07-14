namespace OnibusExpress.Infrastructure.Persistence.Configurations;

public sealed class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
{
    public void Configure(EntityTypeBuilder<Passenger> b)
    {
        b.ToTable("Passengers");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedNever(); // domain generates the Guid

        b.Property(p => p.Name).IsRequired().HasMaxLength(160);
        b.Property(p => p.BirthDate).IsRequired();

        // Email value object <-> string
        b.Property(p => p.Email)
            .HasConversion(v => v.Value, v => Email.Create(v))
            .HasColumnName("Email").HasMaxLength(180).IsRequired();

        // Document value object -> two columns (Document, DocumentType)
        b.OwnsOne(p => p.Document, d =>
        {
            d.Property(x => x.Value).HasColumnName("Document").HasMaxLength(20).IsRequired();
            d.Property(x => x.Type).HasColumnName("DocumentType")
                .HasConversion<string>().HasMaxLength(20).IsRequired();
        });
    }
}

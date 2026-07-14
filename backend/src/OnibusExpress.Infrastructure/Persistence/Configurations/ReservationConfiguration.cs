namespace OnibusExpress.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> b)
    {
        b.ToTable("Reservations");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedNever(); // domain generates the Guid

        b.Property(r => r.TripId).IsRequired();
        b.Property(r => r.DepartureUtc).IsRequired();
        b.Property(r => r.Price).HasColumnType("numeric(10,2)");

        // SeatNumber value object <-> int
        b.Property(r => r.Seat)
            .HasConversion(v => v.Value, v => SeatNumber.Create(v))
            .HasColumnName("Seat").IsRequired();

        // ReservationCode value object <-> string
        b.Property(r => r.Code)
            .HasConversion(v => v.Value, v => ReservationCode.Create(v))
            .HasColumnName("Code").HasMaxLength(9).IsRequired();

        // Status enum <-> string (readable in the DB)
        b.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Relational FK: Reservation -> Passenger (shadow "PassengerId")
        b.HasOne(r => r.Passenger)
            .WithMany()
            .HasForeignKey("PassengerId")
            .OnDelete(DeleteBehavior.Restrict);

        // Unique reservation code
        b.HasIndex(r => r.Code).IsUnique().HasDatabaseName("IX_Reservations_Code");

        // At most one ACTIVE reservation per (Trip, Seat) — the seat-taken invariant at DB level
        b.HasIndex("TripId", "Seat")
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'")
            .HasDatabaseName("IX_Reservations_Trip_Seat_Active");
    }
}

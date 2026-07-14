using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnibusExpress.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Passengers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                Document = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                DocumentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                BirthDate = table.Column<DateOnly>(type: "date", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Passengers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Routes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Origin = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Destination = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                EstimatedDuration = table.Column<TimeSpan>(type: "interval", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Routes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Trips",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                Origin = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Destination = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                DepartureUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                BasePrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                TotalSeats = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Trips", x => x.Id);
                table.ForeignKey(
                    name: "FK_Trips_Routes_RouteId",
                    column: x => x.RouteId,
                    principalTable: "Routes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Reservations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TripId = table.Column<Guid>(type: "uuid", nullable: false),
                PassengerId = table.Column<Guid>(type: "uuid", nullable: false),
                Seat = table.Column<int>(type: "integer", nullable: false),
                Code = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                DepartureUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reservations", x => x.Id);
                table.ForeignKey(
                    name: "FK_Reservations_Passengers_PassengerId",
                    column: x => x.PassengerId,
                    principalTable: "Passengers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Reservations_Trips_TripId",
                    column: x => x.TripId,
                    principalTable: "Trips",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Reservations_Code",
            table: "Reservations",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Reservations_PassengerId",
            table: "Reservations",
            column: "PassengerId");

        migrationBuilder.CreateIndex(
            name: "IX_Reservations_Trip_Seat_Active",
            table: "Reservations",
            columns: new[] { "TripId", "Seat" },
            unique: true,
            filter: "\"Status\" = 'Active'");

        migrationBuilder.CreateIndex(
            name: "IX_Routes_Origin_Destination",
            table: "Routes",
            columns: new[] { "Origin", "Destination" });

        migrationBuilder.CreateIndex(
            name: "IX_Trips_DepartureUtc",
            table: "Trips",
            column: "DepartureUtc");

        migrationBuilder.CreateIndex(
            name: "IX_Trips_Origin_Destination",
            table: "Trips",
            columns: new[] { "Origin", "Destination" });

        migrationBuilder.CreateIndex(
            name: "IX_Trips_RouteId",
            table: "Trips",
            column: "RouteId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Reservations");

        migrationBuilder.DropTable(
            name: "Passengers");

        migrationBuilder.DropTable(
            name: "Trips");

        migrationBuilder.DropTable(
            name: "Routes");
    }
}

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportRim.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationSeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReservationSeats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: false),
                    SeatNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationSeats_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservationSeats_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeats_ReservationId",
                table: "ReservationSeats",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeats_TripId_SeatNumber",
                table: "ReservationSeats",
                columns: new[] { "TripId", "SeatNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationSeats");
        }
    }
}

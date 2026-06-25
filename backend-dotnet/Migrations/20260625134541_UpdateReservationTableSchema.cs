using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportRim.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReservationTableSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SeatNumber",
                table: "Reservations",
                newName: "ReservedSeats");

            migrationBuilder.RenameColumn(
                name: "BookingDate",
                table: "Reservations",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ReservedSeats",
                table: "Reservations",
                newName: "SeatNumber");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Reservations",
                newName: "BookingDate");
        }
    }
}

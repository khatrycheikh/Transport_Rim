using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportRim.Api.Migrations
{
    /// <inheritdoc />
    public partial class UniqueBusNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Buses_BusNumber",
                table: "Buses",
                column: "BusNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buses_BusNumber",
                table: "Buses");
        }
    }
}

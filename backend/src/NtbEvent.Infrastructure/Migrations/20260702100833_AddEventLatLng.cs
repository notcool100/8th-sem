using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventLatLng : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "events",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "events",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "events");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "events");
        }
    }
}

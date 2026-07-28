using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFestivalLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "festivals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "festivals",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "festivals",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "region",
                table: "festivals",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "festivals");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "festivals");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "festivals");

            migrationBuilder.DropColumn(
                name: "region",
                table: "festivals");
        }
    }
}

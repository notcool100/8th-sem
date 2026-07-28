using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizerImageAndFestivalOrganizer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "organizer",
                table: "festivals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "organizer_image_url",
                table: "festivals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "organizer_subtitle",
                table: "festivals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "organizer_verified",
                table: "festivals",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "organizer_image_url",
                table: "events",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "organizer",
                table: "festivals");

            migrationBuilder.DropColumn(
                name: "organizer_image_url",
                table: "festivals");

            migrationBuilder.DropColumn(
                name: "organizer_subtitle",
                table: "festivals");

            migrationBuilder.DropColumn(
                name: "organizer_verified",
                table: "festivals");

            migrationBuilder.DropColumn(
                name: "organizer_image_url",
                table: "events");
        }
    }
}

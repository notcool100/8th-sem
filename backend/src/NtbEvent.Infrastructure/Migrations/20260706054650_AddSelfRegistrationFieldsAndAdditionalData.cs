using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSelfRegistrationFieldsAndAdditionalData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "self_registration_fields",
                table: "events",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "additional_fields",
                table: "event_registrations",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "self_registration_fields",
                table: "events");

            migrationBuilder.DropColumn(
                name: "additional_fields",
                table: "event_registrations");
        }
    }
}

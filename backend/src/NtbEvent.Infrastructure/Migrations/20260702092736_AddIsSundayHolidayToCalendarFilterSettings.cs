using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSundayHolidayToCalendarFilterSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_sunday_holiday",
                table: "calendar_filter_settings",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_sunday_holiday",
                table: "calendar_filter_settings");
        }
    }
}

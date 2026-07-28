using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarFilterSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calendar_filter_settings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    show_category = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_date_range = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_location = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_price = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_tags = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_filter_settings", x => x.id);
                });

            // Seed the default singleton row
            migrationBuilder.InsertData(
                table: "calendar_filter_settings",
                columns: new[] { "id", "show_category", "show_date_range", "show_location", "show_price", "show_tags", "created_at_utc", "updated_at_utc" },
                values: new object[] { 1L, true, true, true, true, true, DateTime.UtcNow, DateTime.UtcNow });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "calendar_filter_settings");
        }
    }
}

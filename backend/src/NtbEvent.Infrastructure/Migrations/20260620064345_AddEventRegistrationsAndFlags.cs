using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventRegistrationsAndFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "requires_invitation",
                table: "events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "requires_registration",
                table: "events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "event_registrations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<long>(type: "bigint", nullable: false),
                    guest_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requested_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    reviewed_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    reviewed_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_registrations", x => x.id);
                    table.ForeignKey(
                        name: "FK_event_registrations_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_registrations_invitation_guests_guest_id",
                        column: x => x.guest_id,
                        principalTable: "invitation_guests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_registrations_users_reviewed_by_user_id",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_registrations_guest_id",
                table: "event_registrations",
                column: "guest_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_registrations_reviewed_by_user_id",
                table: "event_registrations",
                column: "reviewed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_event_registrations_event_guest",
                table: "event_registrations",
                columns: new[] { "event_id", "guest_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_registrations");

            migrationBuilder.DropColumn(
                name: "requires_invitation",
                table: "events");

            migrationBuilder.DropColumn(
                name: "requires_registration",
                table: "events");
        }
    }
}

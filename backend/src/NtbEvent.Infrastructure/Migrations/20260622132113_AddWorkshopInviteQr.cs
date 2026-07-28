using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopInviteQr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "token",
                table: "workshop_invites",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "verified_at_utc",
                table: "workshop_invites",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "verified_by_user_id",
                table: "workshop_invites",
                type: "bigint",
                nullable: true);

            // Backfill a unique token for rows imported before this column existed —
            // they all defaulted to "" above, which would violate the unique index
            // created right after this.
            migrationBuilder.Sql("""
                UPDATE workshop_invites
                SET token = md5(random()::text || clock_timestamp()::text || id::text)
                WHERE token = '' OR token IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_workshop_invites_verified_by_user_id",
                table: "workshop_invites",
                column: "verified_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_workshop_invites_token",
                table: "workshop_invites",
                column: "token",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_workshop_invites_users_verified_by_user_id",
                table: "workshop_invites",
                column: "verified_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_workshop_invites_users_verified_by_user_id",
                table: "workshop_invites");

            migrationBuilder.DropIndex(
                name: "IX_workshop_invites_verified_by_user_id",
                table: "workshop_invites");

            migrationBuilder.DropIndex(
                name: "ux_workshop_invites_token",
                table: "workshop_invites");

            migrationBuilder.DropColumn(
                name: "token",
                table: "workshop_invites");

            migrationBuilder.DropColumn(
                name: "verified_at_utc",
                table: "workshop_invites");

            migrationBuilder.DropColumn(
                name: "verified_by_user_id",
                table: "workshop_invites");
        }
    }
}

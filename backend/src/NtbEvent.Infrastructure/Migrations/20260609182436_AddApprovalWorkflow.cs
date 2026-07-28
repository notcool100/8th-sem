using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "needs_approval",
                table: "user_permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "event_approval_requests",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<long>(type: "bigint", nullable: false),
                    requested_by_user_id = table.Column<long>(type: "bigint", nullable: false),
                    action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    original_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    reviewed_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    requested_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    reviewed_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_approval_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_event_approval_requests_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_approval_requests_users_requested_by_user_id",
                        column: x => x.requested_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_event_approval_requests_users_reviewed_by_user_id",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "idx_event_approvals_status",
                table: "event_approval_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_event_approval_requests_event_id",
                table: "event_approval_requests",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_approval_requests_requested_by_user_id",
                table: "event_approval_requests",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_approval_requests_reviewed_by_user_id",
                table: "event_approval_requests",
                column: "reviewed_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_approval_requests");

            migrationBuilder.DropColumn(
                name: "needs_approval",
                table: "user_permissions");
        }
    }
}

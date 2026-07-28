using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFestivals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "festivals",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: false),
                    long_description = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    date_ad = table.Column<DateTime>(type: "date", nullable: false),
                    end_date_ad = table.Column<DateTime>(type: "date", nullable: false),
                    date_bs = table.Column<string>(type: "text", nullable: false),
                    end_date_bs = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "text", nullable: false),
                    date_range_label = table.Column<string>(type: "text", nullable: false),
                    duration_label = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: false),
                    highlights_json = table.Column<string>(type: "jsonb", nullable: false),
                    featured = table.Column<bool>(type: "boolean", nullable: false),
                    read_time = table.Column<string>(type: "text", nullable: false),
                    created_by_id = table.Column<long>(type: "bigint", nullable: true),
                    updated_by_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_festivals", x => x.id);
                    table.ForeignKey(
                        name: "FK_festivals_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_festivals_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_festivals_status_date",
                table: "festivals",
                columns: new[] { "status", "date_ad" });

            migrationBuilder.CreateIndex(
                name: "IX_festivals_created_by_id",
                table: "festivals",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_festivals_slug",
                table: "festivals",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_festivals_updated_by_id",
                table: "festivals",
                column: "updated_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "festivals");
        }
    }
}

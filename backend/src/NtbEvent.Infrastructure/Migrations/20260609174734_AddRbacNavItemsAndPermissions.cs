using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRbacNavItemsAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nav_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    section = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nav_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_permissions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    nav_item_id = table.Column<int>(type: "integer", nullable: false),
                    can_view = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_create = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_update = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_delete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_permissions_nav_items_nav_item_id",
                        column: x => x.nav_item_id,
                        principalTable: "nav_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_permissions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_nav_items_key",
                table: "nav_items",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_permissions_nav_item_id",
                table: "user_permissions",
                column: "nav_item_id");

            migrationBuilder.CreateIndex(
                name: "ux_user_permissions_user_navitem",
                table: "user_permissions",
                columns: new[] { "user_id", "nav_item_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_permissions");

            migrationBuilder.DropTable(
                name: "nav_items");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class added_tags_categorytag_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tag",
                table: "Categories");

            migrationBuilder.AddColumn<long>(
                name: "TagId",
                table: "Categories",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatagoriesTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    TagId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatagoriesTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatagoriesTags_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatagoriesTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TagId",
                table: "Categories",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_CatagoriesTags_CategoryId",
                table: "CatagoriesTags",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CatagoriesTags_TagId",
                table: "CatagoriesTags",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_CatagoriesTags_TagId",
                table: "Categories",
                column: "TagId",
                principalTable: "CatagoriesTags",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_CatagoriesTags_TagId",
                table: "Categories");

            migrationBuilder.DropTable(
                name: "CatagoriesTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TagId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "TagId",
                table: "Categories");

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "Categories",
                type: "text",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removedTagFromCatagory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_CatagoriesTags_TagId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TagId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "TagId",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TagId",
                table: "Categories",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TagId",
                table: "Categories",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_CatagoriesTags_TagId",
                table: "Categories",
                column: "TagId",
                principalTable: "CatagoriesTags",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NtbEvent.Infrastructure.Persistence;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    [DbContext(typeof(NtbEventDbContext))]
    [Migration("20260703000000_RenameCatagoriesTagsToCategoriesTags")]
    /// <inheritdoc />
    public partial class RenameCatagoriesTagsToCategoriesTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The table was created as "CatagoriesTags" (a typo carried over from the
            // CatagoriesTags entity name), but NtbEventDbContext maps it to the correctly
            // spelled "CategoriesTags". No migration ever renamed the physical table, so
            // every database built purely from migrations (as opposed to an existing dev
            // database that happened to get hand-fixed) fails on every categories query.
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'CatagoriesTags')
                       AND NOT EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'CategoriesTags') THEN
                        ALTER TABLE ""CatagoriesTags"" RENAME TO ""CategoriesTags"";
                        ALTER TABLE ""CategoriesTags"" RENAME CONSTRAINT ""PK_CatagoriesTags"" TO ""PK_CategoriesTags"";
                        ALTER INDEX ""IX_CatagoriesTags_CategoryId"" RENAME TO ""IX_CategoriesTags_CategoryId"";
                        ALTER INDEX ""IX_CatagoriesTags_TagId"" RENAME TO ""IX_CategoriesTags_TagId"";
                        ALTER TABLE ""CategoriesTags"" RENAME CONSTRAINT ""FK_CatagoriesTags_Categories_CategoryId"" TO ""FK_CategoriesTags_Categories_CategoryId"";
                        ALTER TABLE ""CategoriesTags"" RENAME CONSTRAINT ""FK_CatagoriesTags_Tags_TagId"" TO ""FK_CategoriesTags_Tags_TagId"";
                    END IF;
                END
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'CategoriesTags')
                       AND NOT EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'CatagoriesTags') THEN
                        ALTER TABLE ""CategoriesTags"" RENAME TO ""CatagoriesTags"";
                        ALTER TABLE ""CatagoriesTags"" RENAME CONSTRAINT ""PK_CategoriesTags"" TO ""PK_CatagoriesTags"";
                        ALTER INDEX ""IX_CategoriesTags_CategoryId"" RENAME TO ""IX_CatagoriesTags_CategoryId"";
                        ALTER INDEX ""IX_CategoriesTags_TagId"" RENAME TO ""IX_CatagoriesTags_TagId"";
                        ALTER TABLE ""CatagoriesTags"" RENAME CONSTRAINT ""FK_CategoriesTags_Categories_CategoryId"" TO ""FK_CatagoriesTags_Categories_CategoryId"";
                        ALTER TABLE ""CatagoriesTags"" RENAME CONSTRAINT ""FK_CategoriesTags_Tags_TagId"" TO ""FK_CatagoriesTags_Tags_TagId"";
                    END IF;
                END
                $$;
            ");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NtbEvent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsJnAuditTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags_jn",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagId = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    OldName = table.Column<string>(type: "text", nullable: true),
                    NewName = table.Column<string>(type: "text", nullable: true),
                    ChangedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags_jn", x => x.Id);
                });

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION tags_audit_trigger_fn()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF TG_OP = 'INSERT' THEN
                        INSERT INTO ""Tags_jn"" (""TagId"", ""Action"", ""OldName"", ""NewName"", ""ChangedAtUtc"")
                        VALUES (NEW.""Id"", 'INSERT', NULL, NEW.""Name"", NOW());
                        RETURN NEW;
                    ELSIF TG_OP = 'UPDATE' THEN
                        INSERT INTO ""Tags_jn"" (""TagId"", ""Action"", ""OldName"", ""NewName"", ""ChangedAtUtc"")
                        VALUES (NEW.""Id"", 'UPDATE', OLD.""Name"", NEW.""Name"", NOW());
                        RETURN NEW;
                    ELSIF TG_OP = 'DELETE' THEN
                        INSERT INTO ""Tags_jn"" (""TagId"", ""Action"", ""OldName"", ""NewName"", ""ChangedAtUtc"")
                        VALUES (OLD.""Id"", 'DELETE', OLD.""Name"", NULL, NOW());
                        RETURN OLD;
                    END IF;
                    RETURN NULL;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER tags_audit_trigger
                AFTER INSERT OR UPDATE OR DELETE ON ""Tags""
                FOR EACH ROW EXECUTE FUNCTION tags_audit_trigger_fn();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS tags_audit_trigger ON ""Tags"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS tags_audit_trigger_fn();");

            migrationBuilder.DropTable(name: "Tags_jn");
        }
    }
}

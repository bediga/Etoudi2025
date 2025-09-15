using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VcBlazor.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "file_size",
                table: "submission_documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "submission_documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "submission_documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_image",
                table: "submission_documents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "mime_type",
                table: "submission_documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "original_file_name",
                table: "submission_documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "submission_documents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "uploaded_by",
                table: "submission_documents",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_submission_documents_uploaded_by",
                table: "submission_documents",
                column: "uploaded_by");

            migrationBuilder.AddForeignKey(
                name: "FK_submission_documents_users_uploaded_by",
                table: "submission_documents",
                column: "uploaded_by",
                principalTable: "users",
                principalColumn: "id");

            // Insérer l'utilisateur par défaut pour les soumissions anonymes
            migrationBuilder.Sql(@"
                INSERT INTO users (first_name, last_name, email, password, role, created_at, updated_at, is_active, must_change_password)
                VALUES ('Système', 'Anonyme', 'system@example.com', 'hashed_password', 'System', NOW(), NOW(), true, false)
                ON CONFLICT (id) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_submission_documents_users_uploaded_by",
                table: "submission_documents");

            migrationBuilder.DropIndex(
                name: "IX_submission_documents_uploaded_by",
                table: "submission_documents");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "submission_documents");

            migrationBuilder.DropColumn(
                name: "description",
                table: "submission_documents");

            migrationBuilder.DropColumn(
                name: "is_image",
                table: "submission_documents");

            migrationBuilder.DropColumn(
                name: "mime_type",
                table: "submission_documents");

            migrationBuilder.DropColumn(
                name: "original_file_name",
                table: "submission_documents");

            migrationBuilder.DropColumn(
                name: "status",
                table: "submission_documents");

            migrationBuilder.DropColumn(
                name: "uploaded_by",
                table: "submission_documents");

            migrationBuilder.AlterColumn<int>(
                name: "file_size",
                table: "submission_documents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}

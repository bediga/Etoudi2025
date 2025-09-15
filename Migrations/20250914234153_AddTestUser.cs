using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VcBlazor.Migrations
{
    /// <inheritdoc />
    public partial class AddTestUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insérer un utilisateur de test pour le développement
            migrationBuilder.Sql(@"
                INSERT INTO users (id, first_name, last_name, email, phone_number, password, role, is_active, created_at, updated_at, must_change_password)
                VALUES (1, 'Admin', 'Test', 'admin@vc2025.cm', '123456789', '$2a$11$WKzrEo1dWxVPjsVnhJJ4KO7M/UJGJQTkkE8fz7Z8K2zbNz9YlKJmK', 'Administrator', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, false)
                ON CONFLICT (id) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Supprimer l'utilisateur de test
            migrationBuilder.Sql("DELETE FROM users WHERE id = 1;");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetNotepad.AuthService.Migrations
{
    /// <inheritdoc />
    public partial class Auto_20261002_170339 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_UserGuid",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_RefreshToken",
                table: "RefreshTokens",
                column: "RefreshToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_RefreshToken",
                table: "RefreshTokens");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserGuid",
                table: "Users",
                column: "UserGuid");
        }
    }
}

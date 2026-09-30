using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetNotepad.AuthService.Migrations
{
    /// <inheritdoc />
    public partial class Auto_20260929_173133 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    LastUseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ttl = table.Column<TimeSpan>(type: "interval", nullable: false),
                    DeviceName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.RefreshToken);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserGuid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetNotepad.AuthService.Migrations
{
    /// <inheritdoc />
    public partial class Auto_20260930_172039 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ttl",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "UserLogin");

            migrationBuilder.RenameColumn(
                name: "LastUseDate",
                table: "RefreshTokens",
                newName: "ExpireDate");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "RefreshTokenTTL",
                table: "Users",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "Persistent",
                table: "RefreshTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserGuid",
                table: "Users",
                column: "UserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserGuid",
                table: "RefreshTokens",
                column: "UserGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserGuid",
                table: "RefreshTokens",
                column: "UserGuid",
                principalTable: "Users",
                principalColumn: "UserGuid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserGuid",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserGuid",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserGuid",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RefreshTokenTTL",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Persistent",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "UserLogin",
                table: "Users",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "ExpireDate",
                table: "RefreshTokens",
                newName: "LastUseDate");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Ttl",
                table: "RefreshTokens",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}

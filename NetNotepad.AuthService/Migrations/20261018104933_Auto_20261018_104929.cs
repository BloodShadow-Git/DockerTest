using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetNotepad.AuthService.Migrations
{
    /// <inheritdoc />
    public partial class Auto_20261018_104929 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpireDate",
                table: "RefreshTokens",
                newName: "CreationDate");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TokenTTL",
                table: "RefreshTokens",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenTTL",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "RefreshTokens",
                newName: "ExpireDate");
        }
    }
}

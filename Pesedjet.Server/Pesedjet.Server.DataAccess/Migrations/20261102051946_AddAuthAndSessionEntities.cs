using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pesedjet.Server.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthAndSessionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Identity",
                table: "Player",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                schema: "Identity",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsGuest",
                schema: "Identity",
                table: "Player",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "Identity",
                table: "Player",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Player_Email",
                schema: "Identity",
                table: "Player",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Player_Username",
                schema: "Identity",
                table: "Player",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Player_Email",
                schema: "Identity",
                table: "Player");

            migrationBuilder.DropIndex(
                name: "IX_Player_Username",
                schema: "Identity",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Identity",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                schema: "Identity",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "IsGuest",
                schema: "Identity",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Identity",
                table: "Player");
        }
    }
}

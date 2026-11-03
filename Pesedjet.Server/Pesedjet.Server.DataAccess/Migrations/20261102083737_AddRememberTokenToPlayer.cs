using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pesedjet.Server.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRememberTokenToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RememberToken",
                schema: "Identity",
                table: "Player",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RememberToken",
                schema: "Identity",
                table: "Player");
        }
    }
}

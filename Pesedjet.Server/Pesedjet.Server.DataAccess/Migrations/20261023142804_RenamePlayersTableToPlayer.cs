using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pesedjet.Server.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenamePlayersTableToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlockedPlayer_Players_BlockedId",
                schema: "Identity",
                table: "BlockedPlayer");

            migrationBuilder.DropForeignKey(
                name: "FK_BlockedPlayer_Players_PlayerId",
                schema: "Identity",
                table: "BlockedPlayer");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_Players_PlayerAddresseeId",
                schema: "Identity",
                table: "Friendship");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_Players_PlayerRequesterId",
                schema: "Identity",
                table: "Friendship");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchResult_Players_PlayerId",
                schema: "Game",
                table: "MatchResult");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Players_PlayerId",
                schema: "Identity",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_VerificationCode_Players_PlayerId",
                schema: "Identity",
                table: "VerificationCode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                schema: "Identity",
                table: "Players");

            migrationBuilder.RenameTable(
                name: "Players",
                schema: "Identity",
                newName: "Player",
                newSchema: "Identity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Player",
                schema: "Identity",
                table: "Player",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlockedPlayer_Player_BlockedId",
                schema: "Identity",
                table: "BlockedPlayer",
                column: "BlockedId",
                principalSchema: "Identity",
                principalTable: "Player",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlockedPlayer_Player_PlayerId",
                schema: "Identity",
                table: "BlockedPlayer",
                column: "PlayerId",
                principalSchema: "Identity",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_Player_PlayerAddresseeId",
                schema: "Identity",
                table: "Friendship",
                column: "PlayerAddresseeId",
                principalSchema: "Identity",
                principalTable: "Player",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_Player_PlayerRequesterId",
                schema: "Identity",
                table: "Friendship",
                column: "PlayerRequesterId",
                principalSchema: "Identity",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResult_Player_PlayerId",
                schema: "Game",
                table: "MatchResult",
                column: "PlayerId",
                principalSchema: "Identity",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Player_PlayerId",
                schema: "Identity",
                table: "Session",
                column: "PlayerId",
                principalSchema: "Identity",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationCode_Player_PlayerId",
                schema: "Identity",
                table: "VerificationCode",
                column: "PlayerId",
                principalSchema: "Identity",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlockedPlayer_Player_BlockedId",
                schema: "Identity",
                table: "BlockedPlayer");

            migrationBuilder.DropForeignKey(
                name: "FK_BlockedPlayer_Player_PlayerId",
                schema: "Identity",
                table: "BlockedPlayer");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_Player_PlayerAddresseeId",
                schema: "Identity",
                table: "Friendship");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_Player_PlayerRequesterId",
                schema: "Identity",
                table: "Friendship");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchResult_Player_PlayerId",
                schema: "Game",
                table: "MatchResult");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Player_PlayerId",
                schema: "Identity",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_VerificationCode_Player_PlayerId",
                schema: "Identity",
                table: "VerificationCode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Player",
                schema: "Identity",
                table: "Player");

            migrationBuilder.RenameTable(
                name: "Player",
                schema: "Identity",
                newName: "Players",
                newSchema: "Identity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                schema: "Identity",
                table: "Players",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlockedPlayer_Players_BlockedId",
                schema: "Identity",
                table: "BlockedPlayer",
                column: "BlockedId",
                principalSchema: "Identity",
                principalTable: "Players",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlockedPlayer_Players_PlayerId",
                schema: "Identity",
                table: "BlockedPlayer",
                column: "PlayerId",
                principalSchema: "Identity",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_Players_PlayerAddresseeId",
                schema: "Identity",
                table: "Friendship",
                column: "PlayerAddresseeId",
                principalSchema: "Identity",
                principalTable: "Players",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_Players_PlayerRequesterId",
                schema: "Identity",
                table: "Friendship",
                column: "PlayerRequesterId",
                principalSchema: "Identity",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResult_Players_PlayerId",
                schema: "Game",
                table: "MatchResult",
                column: "PlayerId",
                principalSchema: "Identity",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Players_PlayerId",
                schema: "Identity",
                table: "Session",
                column: "PlayerId",
                principalSchema: "Identity",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationCode_Players_PlayerId",
                schema: "Identity",
                table: "VerificationCode",
                column: "PlayerId",
                principalSchema: "Identity",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

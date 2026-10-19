using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pesedjet.Server.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.EnsureSchema(
                name: "Game");

            migrationBuilder.CreateTable(
                name: "Match",
                schema: "Game",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlayerCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Match", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProfileDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlockedPlayer",
                schema: "Identity",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedPlayer", x => new { x.PlayerId, x.BlockedId });
                    table.ForeignKey(
                        name: "FK_BlockedPlayer_Players_BlockedId",
                        column: x => x.BlockedId,
                        principalSchema: "Identity",
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BlockedPlayer_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "Identity",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Friendship",
                schema: "Identity",
                columns: table => new
                {
                    PlayerRequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerAddresseeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendship", x => new { x.PlayerRequesterId, x.PlayerAddresseeId });
                    table.ForeignKey(
                        name: "FK_Friendship_Players_PlayerAddresseeId",
                        column: x => x.PlayerAddresseeId,
                        principalSchema: "Identity",
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Friendship_Players_PlayerRequesterId",
                        column: x => x.PlayerRequesterId,
                        principalSchema: "Identity",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchResult",
                schema: "Game",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Amulets = table.Column<int>(type: "int", nullable: false),
                    IsWinner = table.Column<bool>(type: "bit", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    ExitReason = table.Column<int>(type: "int", nullable: false),
                    ExpulsionReason = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchResult_Match_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "Game",
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchResult_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "Identity",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Session",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Device = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Session_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "Identity",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificationCode",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationCode_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "Identity",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedPlayer_BlockedId",
                schema: "Identity",
                table: "BlockedPlayer",
                column: "BlockedId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendship_PlayerAddresseeId",
                schema: "Identity",
                table: "Friendship",
                column: "PlayerAddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResult_MatchId",
                schema: "Game",
                table: "MatchResult",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResult_PlayerId",
                schema: "Game",
                table: "MatchResult",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_PlayerId",
                schema: "Identity",
                table: "Session",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCode_PlayerId",
                schema: "Identity",
                table: "VerificationCode",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedPlayer",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Friendship",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "MatchResult",
                schema: "Game");

            migrationBuilder.DropTable(
                name: "Session",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "VerificationCode",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Match",
                schema: "Game");

            migrationBuilder.DropTable(
                name: "Players",
                schema: "Identity");
        }
    }
}

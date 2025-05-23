using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Division = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => new { x.Id, x.Division });
                });

            migrationBuilder.CreateTable(
                name: "PlayerTournaments",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    Division = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Place = table.Column<int>(type: "int", nullable: false),
                    TotalToPar = table.Column<int>(type: "int", nullable: false),
                    Fairway = table.Column<double>(type: "float", nullable: false),
                    C1InReg = table.Column<double>(type: "float", nullable: false),
                    C2InReg = table.Column<double>(type: "float", nullable: false),
                    Parked = table.Column<double>(type: "float", nullable: false),
                    Scramble = table.Column<double>(type: "float", nullable: false),
                    C1Putting = table.Column<double>(type: "float", nullable: false),
                    C1xPutting = table.Column<double>(type: "float", nullable: false),
                    C2Putting = table.Column<double>(type: "float", nullable: false),
                    ObRate = table.Column<double>(type: "float", nullable: false),
                    BirdieMinus = table.Column<double>(type: "float", nullable: false),
                    DoubleBogeyPlus = table.Column<double>(type: "float", nullable: false),
                    BogeyPlus = table.Column<double>(type: "float", nullable: false),
                    Par = table.Column<double>(type: "float", nullable: false),
                    Birdie = table.Column<double>(type: "float", nullable: false),
                    EagleMinus = table.Column<double>(type: "float", nullable: false),
                    PuttDistance = table.Column<int>(type: "int", nullable: false),
                    StrokesGainedTotal = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedTeeToGreen = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedPutting = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedC1xPutting = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedC2Putting = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTournaments", x => new { x.PlayerId, x.TournamentId, x.Division });
                    table.ForeignKey(
                        name: "FK_PlayerTournaments_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerTournaments_Tournaments_TournamentId_Division",
                        columns: x => new { x.TournamentId, x.Division },
                        principalTable: "Tournaments",
                        principalColumns: new[] { "Id", "Division" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    RoundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    Division = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.RoundId);
                    table.ForeignKey(
                        name: "FK_Rounds_Tournaments_TournamentId_Division",
                        columns: x => new { x.TournamentId, x.Division },
                        principalTable: "Tournaments",
                        principalColumns: new[] { "Id", "Division" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoundScores",
                columns: table => new
                {
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Division = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RunningPlace = table.Column<int>(type: "int", nullable: false),
                    RoundToPar = table.Column<int>(type: "int", nullable: false),
                    RunningToPar = table.Column<int>(type: "int", nullable: false),
                    Fairway = table.Column<double>(type: "float", nullable: false),
                    C1InReg = table.Column<double>(type: "float", nullable: false),
                    C2InReg = table.Column<double>(type: "float", nullable: false),
                    Parked = table.Column<double>(type: "float", nullable: false),
                    Scramble = table.Column<double>(type: "float", nullable: false),
                    C1Putting = table.Column<double>(type: "float", nullable: false),
                    C1xPutting = table.Column<double>(type: "float", nullable: false),
                    C2Putting = table.Column<double>(type: "float", nullable: false),
                    ObRate = table.Column<double>(type: "float", nullable: false),
                    BirdieMinus = table.Column<double>(type: "float", nullable: false),
                    DoubleBogeyPlus = table.Column<double>(type: "float", nullable: false),
                    BogeyPlus = table.Column<double>(type: "float", nullable: false),
                    Par = table.Column<double>(type: "float", nullable: false),
                    Birdie = table.Column<double>(type: "float", nullable: false),
                    EagleMinus = table.Column<double>(type: "float", nullable: false),
                    PuttDistance = table.Column<int>(type: "int", nullable: false),
                    StrokesGainedTotal = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedTeeToGreen = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedPutting = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedC1xPutting = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedC2Putting = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundScores", x => new { x.RoundId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_RoundScores_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoundScores_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "RoundId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTournaments_TournamentId_Division",
                table: "PlayerTournaments",
                columns: new[] { "TournamentId", "Division" });

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TournamentId_Division",
                table: "Rounds",
                columns: new[] { "TournamentId", "Division" });

            migrationBuilder.CreateIndex(
                name: "IX_RoundScores_PlayerId",
                table: "RoundScores",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerTournaments");

            migrationBuilder.DropTable(
                name: "RoundScores");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropTable(
                name: "Tournaments");
        }
    }
}

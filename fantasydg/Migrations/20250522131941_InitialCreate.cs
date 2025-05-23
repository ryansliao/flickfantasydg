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
                name: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Division = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rounds = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => new { x.Id, x.Division });
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    RoundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Exists = table.Column<bool>(type: "bit", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: true),
                    Division = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TournamentDivision = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.RoundId);
                    table.ForeignKey(
                        name: "FK_Rounds_Tournaments_TournamentId_TournamentDivision",
                        columns: x => new { x.TournamentId, x.TournamentDivision },
                        principalTable: "Tournaments",
                        principalColumns: new[] { "Id", "Division" });
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place = table.Column<int>(type: "int", nullable: false),
                    TournamentScore = table.Column<int>(type: "int", nullable: false),
                    RoundScore = table.Column<int>(type: "int", nullable: false),
                    Fairway = table.Column<double>(type: "float", nullable: false),
                    C1InReg = table.Column<double>(type: "float", nullable: false),
                    C2InReg = table.Column<double>(type: "float", nullable: false),
                    Parked = table.Column<double>(type: "float", nullable: false),
                    Scramble = table.Column<double>(type: "float", nullable: false),
                    C1Putting = table.Column<double>(type: "float", nullable: false),
                    C1xPutting = table.Column<double>(type: "float", nullable: false),
                    C2Putting = table.Column<double>(type: "float", nullable: false),
                    ObRate = table.Column<double>(type: "float", nullable: false),
                    BirdiePlus = table.Column<double>(type: "float", nullable: false),
                    DoubleBogeyPlus = table.Column<double>(type: "float", nullable: false),
                    BogeyPlus = table.Column<double>(type: "float", nullable: false),
                    Par = table.Column<double>(type: "float", nullable: false),
                    Birdie = table.Column<double>(type: "float", nullable: false),
                    EaglePlus = table.Column<double>(type: "float", nullable: false),
                    PuttDistance = table.Column<int>(type: "int", nullable: false),
                    StrokesGainedTotal = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedTeeToGreen = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedPutting = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedC1xPutting = table.Column<double>(type: "float", nullable: false),
                    StrokesGainedC2Putting = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => new { x.Id, x.RoundId });
                    table.ForeignKey(
                        name: "FK_Players_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Rounds",
                        principalColumn: "RoundId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_RoundId",
                table: "Players",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TournamentId_TournamentDivision",
                table: "Rounds",
                columns: new[] { "TournamentId", "TournamentDivision" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropTable(
                name: "Tournaments");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class SchemaChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Rounds_RoundId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_TournamentDivision",
                table: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_TournamentId_TournamentDivision",
                table: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_RoundId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "Exists",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "TournamentDivision",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "RoundId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Birdie",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "BirdiePlus",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "BogeyPlus",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "C1InReg",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "C1Putting",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "C1xPutting",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "C2InReg",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "C2Putting",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DoubleBogeyPlus",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "EaglePlus",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Fairway",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ObRate",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Par",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Parked",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Place",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PuttDistance",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "RoundScore",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Scramble",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "StrokesGainedC1xPutting",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "StrokesGainedC2Putting",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "StrokesGainedPutting",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "StrokesGainedTeeToGreen",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "StrokesGainedTotal",
                table: "Players");

            migrationBuilder.RenameColumn(
                name: "Rounds",
                table: "Tournaments",
                newName: "RoundNumber");

            migrationBuilder.RenameColumn(
                name: "TournamentScore",
                table: "Players",
                newName: "PlayerId");

            migrationBuilder.AlterColumn<string>(
                name: "Division",
                table: "Tournaments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "Rounds",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                column: "PlayerId");

            migrationBuilder.CreateTable(
                name: "PlayerTournaments",
                columns: table => new
                {
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PlayerTournaments", x => new { x.PlayerId, x.TournamentId, x.Division });
                    table.ForeignKey(
                        name: "FK_PlayerTournaments_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerTournaments_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
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
                name: "IX_Rounds_TournamentId",
                table: "Rounds",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTournaments_TournamentId",
                table: "PlayerTournaments",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoundScores_PlayerId",
                table: "RoundScores",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId",
                table: "Rounds",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId",
                table: "Rounds");

            migrationBuilder.DropTable(
                name: "PlayerTournaments");

            migrationBuilder.DropTable(
                name: "RoundScores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_TournamentId",
                table: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "Players");

            migrationBuilder.RenameColumn(
                name: "RoundNumber",
                table: "Tournaments",
                newName: "Rounds");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                table: "Players",
                newName: "TournamentScore");

            migrationBuilder.AlterColumn<string>(
                name: "Division",
                table: "Tournaments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "Rounds",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "Rounds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Exists",
                table: "Rounds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TournamentDivision",
                table: "Rounds",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<int>(
                name: "RoundId",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddColumn<double>(
                name: "Birdie",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BirdiePlus",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BogeyPlus",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C1InReg",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C1Putting",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C1xPutting",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C2InReg",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C2Putting",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DoubleBogeyPlus",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "EaglePlus",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Fairway",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ObRate",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Par",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Parked",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Place",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PuttDistance",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoundScore",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Scramble",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StrokesGainedC1xPutting",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StrokesGainedC2Putting",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StrokesGainedPutting",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StrokesGainedTeeToGreen",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StrokesGainedTotal",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments",
                columns: new[] { "Id", "Division" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                columns: new[] { "Id", "RoundId" });

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TournamentId_TournamentDivision",
                table: "Rounds",
                columns: new[] { "TournamentId", "TournamentDivision" });

            migrationBuilder.CreateIndex(
                name: "IX_Players_RoundId",
                table: "Players",
                column: "RoundId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Rounds_RoundId",
                table: "Players",
                column: "RoundId",
                principalTable: "Rounds",
                principalColumn: "RoundId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_TournamentDivision",
                table: "Rounds",
                columns: new[] { "TournamentId", "TournamentDivision" },
                principalTable: "Tournaments",
                principalColumns: new[] { "Id", "Division" });
        }
    }
}

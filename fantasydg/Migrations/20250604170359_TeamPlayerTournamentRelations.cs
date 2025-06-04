using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class TeamPlayerTournamentRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamPlayerTournaments",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    PDGANumber = table.Column<int>(type: "int", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    Division = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamPlayerTournaments", x => new { x.TeamId, x.PDGANumber, x.TournamentId, x.Division });
                    table.ForeignKey(
                        name: "FK_TeamPlayerTournaments_Players_PDGANumber",
                        column: x => x.PDGANumber,
                        principalTable: "Players",
                        principalColumn: "PDGANumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamPlayerTournaments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamPlayerTournaments_Tournaments_TournamentId_Division",
                        columns: x => new { x.TournamentId, x.Division },
                        principalTable: "Tournaments",
                        principalColumns: new[] { "Id", "Division" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamPlayerTournaments_PDGANumber",
                table: "TeamPlayerTournaments",
                column: "PDGANumber");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPlayerTournaments_TournamentId_Division",
                table: "TeamPlayerTournaments",
                columns: new[] { "TournamentId", "Division" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamPlayerTournaments");
        }
    }
}

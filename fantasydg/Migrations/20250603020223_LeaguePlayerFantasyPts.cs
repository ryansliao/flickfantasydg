using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class LeaguePlayerFantasyPts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaguePlayerFantasyPoints",
                columns: table => new
                {
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    LeaguePlayerId = table.Column<int>(type: "int", nullable: false),
                    Division = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Points = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaguePlayerFantasyPoints", x => new { x.LeagueId, x.PlayerId, x.TournamentId });
                    table.ForeignKey(
                        name: "FK_LeaguePlayerFantasyPoints_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaguePlayerFantasyPoints_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaguePlayerFantasyPoints_Tournaments_TournamentId_Division",
                        columns: x => new { x.TournamentId, x.Division },
                        principalTable: "Tournaments",
                        principalColumns: new[] { "Id", "Division" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaguePlayerFantasyPoints_PlayerId",
                table: "LeaguePlayerFantasyPoints",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaguePlayerFantasyPoints_TournamentId_Division",
                table: "LeaguePlayerFantasyPoints",
                columns: new[] { "TournamentId", "Division" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaguePlayerFantasyPoints");
        }
    }
}

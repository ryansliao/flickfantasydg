using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class LeagueTournament : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Tournaments");

            migrationBuilder.CreateTable(
                name: "LeagueTournaments",
                columns: table => new
                {
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    Division = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false, defaultValue: 1.0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueTournaments", x => new { x.LeagueId, x.TournamentId, x.Division });
                    table.ForeignKey(
                        name: "FK_LeagueTournaments_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueTournaments_Tournaments_TournamentId_Division",
                        columns: x => new { x.TournamentId, x.Division },
                        principalTable: "Tournaments",
                        principalColumns: new[] { "Id", "Division" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueTournaments_TournamentId_Division",
                table: "LeagueTournaments",
                columns: new[] { "TournamentId", "Division" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeagueTournaments");

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "Tournaments",
                type: "float",
                nullable: false,
                defaultValue: 1.0);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class TournamentIDNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_TournamentDivision",
                table: "Rounds");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "Rounds",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "TournamentDivision",
                table: "Rounds",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_TournamentDivision",
                table: "Rounds",
                columns: new[] { "TournamentId", "TournamentDivision" },
                principalTable: "Tournaments",
                principalColumns: new[] { "Id", "Division" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_TournamentDivision",
                table: "Rounds");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "Rounds",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TournamentDivision",
                table: "Rounds",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_TournamentDivision",
                table: "Rounds",
                columns: new[] { "TournamentId", "TournamentDivision" },
                principalTable: "Tournaments",
                principalColumns: new[] { "Id", "Division" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}

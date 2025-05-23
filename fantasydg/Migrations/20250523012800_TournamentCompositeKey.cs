using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class TournamentCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerTournaments_Tournaments_TournamentId",
                table: "PlayerTournaments");

            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId",
                table: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_TournamentId",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_PlayerTournaments_TournamentId",
                table: "PlayerTournaments");

            migrationBuilder.AlterColumn<string>(
                name: "Division",
                table: "Tournaments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Tournaments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "Rounds",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments",
                columns: new[] { "Id", "Division" });

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TournamentId_Division",
                table: "Rounds",
                columns: new[] { "TournamentId", "Division" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTournaments_TournamentId_Division",
                table: "PlayerTournaments",
                columns: new[] { "TournamentId", "Division" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerTournaments_Tournaments_TournamentId_Division",
                table: "PlayerTournaments",
                columns: new[] { "TournamentId", "Division" },
                principalTable: "Tournaments",
                principalColumns: new[] { "Id", "Division" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_Division",
                table: "Rounds",
                columns: new[] { "TournamentId", "Division" },
                principalTable: "Tournaments",
                principalColumns: new[] { "Id", "Division" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerTournaments_Tournaments_TournamentId_Division",
                table: "PlayerTournaments");

            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_Division",
                table: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_TournamentId_Division",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_PlayerTournaments_TournamentId_Division",
                table: "PlayerTournaments");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "Rounds");

            migrationBuilder.AlterColumn<string>(
                name: "Division",
                table: "Tournaments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Tournaments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TournamentId",
                table: "Rounds",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTournaments_TournamentId",
                table: "PlayerTournaments",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerTournaments_Tournaments_TournamentId",
                table: "PlayerTournaments",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId",
                table: "Rounds",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

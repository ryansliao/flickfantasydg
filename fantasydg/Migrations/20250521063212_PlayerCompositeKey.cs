using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class PlayerCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_Division",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_TournamentId_Division",
                table: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                table: "Players");

            migrationBuilder.AlterColumn<string>(
                name: "Division",
                table: "Rounds",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "TournamentDivision",
                table: "Rounds",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "RoundId",
                table: "Players",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Players",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                columns: new[] { "Id", "RoundId" });

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TournamentId_TournamentDivision",
                table: "Rounds",
                columns: new[] { "TournamentId", "TournamentDivision" });

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_TournamentDivision",
                table: "Rounds",
                columns: new[] { "TournamentId", "TournamentDivision" },
                principalTable: "Tournaments",
                principalColumns: new[] { "Id", "Division" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_TournamentDivision",
                table: "Rounds");

            migrationBuilder.DropIndex(
                name: "IX_Rounds_TournamentId_TournamentDivision",
                table: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "TournamentDivision",
                table: "Rounds");

            migrationBuilder.AlterColumn<string>(
                name: "Division",
                table: "Rounds",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "RoundId",
                table: "Players",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Players",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TournamentId_Division",
                table: "Rounds",
                columns: new[] { "TournamentId", "Division" });

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId_Division",
                table: "Rounds",
                columns: new[] { "TournamentId", "Division" },
                principalTable: "Tournaments",
                principalColumns: new[] { "Id", "Division" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}

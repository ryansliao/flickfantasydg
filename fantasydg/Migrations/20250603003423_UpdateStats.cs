using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PuttDistance",
                table: "RoundScores",
                newName: "TotalPuttDistance");

            migrationBuilder.RenameColumn(
                name: "PuttDistance",
                table: "PlayerTournaments",
                newName: "TotalPuttDistance");

            migrationBuilder.AddColumn<double>(
                name: "AvgPuttDistance",
                table: "RoundScores",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "LongThrowIn",
                table: "RoundScores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "AvgPuttDistance",
                table: "PlayerTournaments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "LongThrowIn",
                table: "PlayerTournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgPuttDistance",
                table: "RoundScores");

            migrationBuilder.DropColumn(
                name: "LongThrowIn",
                table: "RoundScores");

            migrationBuilder.DropColumn(
                name: "AvgPuttDistance",
                table: "PlayerTournaments");

            migrationBuilder.DropColumn(
                name: "LongThrowIn",
                table: "PlayerTournaments");

            migrationBuilder.RenameColumn(
                name: "TotalPuttDistance",
                table: "RoundScores",
                newName: "PuttDistance");

            migrationBuilder.RenameColumn(
                name: "TotalPuttDistance",
                table: "PlayerTournaments",
                newName: "PuttDistance");
        }
    }
}

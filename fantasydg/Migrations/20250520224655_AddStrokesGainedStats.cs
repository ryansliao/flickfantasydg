using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class AddStrokesGainedStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class ScoringSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AvgPuttDistWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BirdiePlusWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BirdieWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BogeyPlusWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C1InRegWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C1xPuttWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C1xSGWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C2InRegWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C2PuttWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C2SGWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DoubleBogeyPlusWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "EaglePlusWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FairwayWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LongThrowInWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OBWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ParWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ParkedWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PuttingSGWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ScrambleWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TeeToGreenSGWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalPuttDistWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalSGWeight",
                table: "Leagues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgPuttDistWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "BirdiePlusWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "BirdieWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "BogeyPlusWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "C1InRegWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "C1xPuttWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "C1xSGWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "C2InRegWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "C2PuttWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "C2SGWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "DoubleBogeyPlusWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "EaglePlusWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "FairwayWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "LongThrowInWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "OBWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "ParWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "ParkedWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "PuttingSGWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "ScrambleWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "TeeToGreenSGWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "TotalPuttDistWeight",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "TotalSGWeight",
                table: "Leagues");
        }
    }
}

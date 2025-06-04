using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class RosterSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InjuryReserveLimit",
                table: "Leagues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RosterLimit",
                table: "Leagues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StarterCount",
                table: "Leagues",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InjuryReserveLimit",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "RosterLimit",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "StarterCount",
                table: "Leagues");
        }
    }
}

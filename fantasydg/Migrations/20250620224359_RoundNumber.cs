using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class RoundNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Tournaments");

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "Tournaments");

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "Tournaments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}

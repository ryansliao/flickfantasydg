using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class RosterSpots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TeamPlayers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TeamPlayers");
        }
    }
}

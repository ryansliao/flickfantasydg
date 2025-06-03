using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScoringSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EaglePlusWeight",
                table: "Leagues",
                newName: "EagleMinusWeight");

            migrationBuilder.RenameColumn(
                name: "BirdiePlusWeight",
                table: "Leagues",
                newName: "BirdieMinusWeight");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EagleMinusWeight",
                table: "Leagues",
                newName: "EaglePlusWeight");

            migrationBuilder.RenameColumn(
                name: "BirdieMinusWeight",
                table: "Leagues",
                newName: "BirdiePlusWeight");
        }
    }
}

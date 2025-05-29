using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class RenameLeagueCreatorToOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leagues_AspNetUsers_CreatorId",
                table: "Leagues");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Leagues",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Leagues_CreatorId",
                table: "Leagues",
                newName: "IX_Leagues_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leagues_AspNetUsers_OwnerId",
                table: "Leagues",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leagues_AspNetUsers_OwnerId",
                table: "Leagues");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Leagues",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Leagues_OwnerId",
                table: "Leagues",
                newName: "IX_Leagues_CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leagues_AspNetUsers_CreatorId",
                table: "Leagues",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

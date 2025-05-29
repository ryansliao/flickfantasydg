using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class TransferOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeagueOwnershipTransfers",
                columns: table => new
                {
                    LeagueOwnershipTransferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    NewOwnerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueOwnershipTransfers", x => x.LeagueOwnershipTransferId);
                    table.ForeignKey(
                        name: "FK_LeagueOwnershipTransfers_AspNetUsers_NewOwnerId",
                        column: x => x.NewOwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueOwnershipTransfers_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueOwnershipTransfers_LeagueId",
                table: "LeagueOwnershipTransfers",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueOwnershipTransfers_NewOwnerId",
                table: "LeagueOwnershipTransfers",
                column: "NewOwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeagueOwnershipTransfers");
        }
    }
}

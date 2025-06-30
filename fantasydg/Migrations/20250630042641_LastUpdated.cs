using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fantasydg.Migrations
{
    /// <inheritdoc />
    public partial class LastUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedTime",
                table: "Tournaments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdatedTime",
                table: "Tournaments");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.MarketplaceService.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInterestReadTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InitiatorReadAt",
                table: "Interests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetReadAt",
                table: "Interests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitiatorReadAt",
                table: "Interests");

            migrationBuilder.DropColumn(
                name: "TargetReadAt",
                table: "Interests");
        }
    }
}

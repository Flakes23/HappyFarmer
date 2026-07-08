using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.MarketplaceService.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerTrustInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FarmerJoinedAt",
                table: "Listings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FarmerName",
                table: "Listings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BuyerJoinedAt",
                table: "BuyRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                table: "BuyRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FarmerJoinedAt",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "FarmerName",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "BuyerJoinedAt",
                table: "BuyRequests");

            migrationBuilder.DropColumn(
                name: "BuyerName",
                table: "BuyRequests");
        }
    }
}

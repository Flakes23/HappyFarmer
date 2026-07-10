using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.MarketplaceService.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmerBuyerAvatarUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FarmerAvatarUrl",
                table: "Listings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerAvatarUrl",
                table: "BuyRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FarmerAvatarUrl",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "BuyerAvatarUrl",
                table: "BuyRequests");
        }
    }
}

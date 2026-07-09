using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.MarketPriceService.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceEntryVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Variant",
                table: "PriceEntries",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Variant",
                table: "PriceEntries");
        }
    }
}

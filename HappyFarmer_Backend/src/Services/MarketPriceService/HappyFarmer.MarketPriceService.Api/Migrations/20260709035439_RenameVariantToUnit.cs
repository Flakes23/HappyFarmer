using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.MarketPriceService.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameVariantToUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Variant",
                table: "PriceEntries",
                newName: "Unit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "PriceEntries",
                newName: "Variant");
        }
    }
}

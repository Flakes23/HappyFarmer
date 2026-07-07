using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.MarketPriceService.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixCrawlerRegionMarketNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 103,
                column: "MarketName",
                value: "Giá tham khảo tỉnh Gia Lai");

            migrationBuilder.UpdateData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 104,
                column: "MarketName",
                value: "Giá tham khảo tỉnh Đồng Nai");

            migrationBuilder.UpdateData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 105,
                column: "MarketName",
                value: "Giá tham khảo tỉnh Đắk Nông");

            migrationBuilder.UpdateData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 106,
                column: "MarketName",
                value: "Giá tham khảo tỉnh Đắk Lắk");

            migrationBuilder.UpdateData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 107,
                column: "MarketName",
                value: "Giá tham khảo tỉnh Lâm Đồng");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 103,
                column: "MarketName",
                value: "Giá tham khảo tỉnh");

            migrationBuilder.UpdateData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 104,
                column: "MarketName",
                value: "Giá tham khảo tỉnh");

            migrationBuilder.UpdateData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 105,
                column: "MarketName",
                value: "Giá tham khảo tỉnh");

            migrationBuilder.UpdateData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 106,
                column: "MarketName",
                value: "Giá tham khảo tỉnh");

            migrationBuilder.UpdateData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 107,
                column: "MarketName",
                value: "Giá tham khảo tỉnh");
        }
    }
}

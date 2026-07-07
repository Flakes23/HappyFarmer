using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HappyFarmer.MarketPriceService.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedCrawlerCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "ImageUrl", "NameVi", "Unit" },
                values: new object[,]
                {
                    { 101, "Rau củ quả", null, "Xà lách", "kg" },
                    { 102, "Rau củ quả", null, "Rau diếp cá", "kg" },
                    { 103, "Rau củ quả", null, "Cải ngọt", "kg" },
                    { 104, "Rau củ quả", null, "Cải bẹ xanh", "kg" },
                    { 105, "Rau củ quả", null, "Rau muống", "kg" },
                    { 106, "Rau củ quả", null, "Bí đao", "kg" },
                    { 107, "Rau củ quả", null, "Dưa leo", "kg" },
                    { 108, "Rau củ quả", null, "Hành lá", "kg" },
                    { 109, "Rau củ quả", null, "Nấm rơm", "kg" },
                    { 110, "Rau củ quả", null, "Đậu bắp", "kg" },
                    { 111, "Nông sản công nghiệp", null, "Hồ tiêu", "kg" },
                    { 112, "Nông sản công nghiệp", null, "Cà phê", "kg" }
                });

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "Id", "Lat", "Lon", "MarketName", "ProvinceName" },
                values: new object[,]
                {
                    { 101, null, null, "Chợ Vĩnh Long", "Vĩnh Long" },
                    { 102, null, null, "Chợ đầu mối (tổng hợp)", "TP. Hồ Chí Minh" },
                    { 103, null, null, "Giá tham khảo tỉnh", "Gia Lai" },
                    { 104, null, null, "Giá tham khảo tỉnh", "Đồng Nai" },
                    { 105, null, null, "Giá tham khảo tỉnh", "Đắk Nông" },
                    { 106, null, null, "Giá tham khảo tỉnh", "Đắk Lắk" },
                    { 107, null, null, "Giá tham khảo tỉnh", "Lâm Đồng" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 107);
        }
    }
}

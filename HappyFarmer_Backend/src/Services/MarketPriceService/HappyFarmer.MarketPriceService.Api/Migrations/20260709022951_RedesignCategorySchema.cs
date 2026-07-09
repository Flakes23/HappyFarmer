using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HappyFarmer.MarketPriceService.Api.Migrations
{
    /// <inheritdoc />
    public partial class RedesignCategorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reset sạch toàn bộ dữ liệu cũ theo yêu cầu (không giữ gì cũ) — kể cả vài dòng test
            // thủ công có sẵn từ trước (Products/Regions Id 1-3/1-2, không nằm trong danh sách
            // DeleteData tự sinh bên dưới vì EF chỉ track được các dòng seed qua HasData).
            migrationBuilder.Sql("DELETE FROM PriceEntries;");
            migrationBuilder.Sql("DELETE FROM Products;");
            migrationBuilder.Sql("DELETE FROM Regions;");

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

            migrationBuilder.DeleteData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "Regions",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "SubCategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_ProvinceName_MarketName",
                table: "Regions",
                columns: new[] { "ProvinceName", "MarketName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_NameVi",
                table: "Products",
                column: "NameVi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SubCategoryId",
                table: "Products",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryId_Name",
                table: "SubCategories",
                columns: new[] { "CategoryId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SubCategories_SubCategoryId",
                table: "Products",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_SubCategories_SubCategoryId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Regions_ProvinceName_MarketName",
                table: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Products_NameVi",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SubCategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

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
                    { 103, null, null, "Giá tham khảo tỉnh Gia Lai", "Gia Lai" },
                    { 104, null, null, "Giá tham khảo tỉnh Đồng Nai", "Đồng Nai" },
                    { 105, null, null, "Giá tham khảo tỉnh Đắk Nông", "Đắk Nông" },
                    { 106, null, null, "Giá tham khảo tỉnh Đắk Lắk", "Đắk Lắk" },
                    { 107, null, null, "Giá tham khảo tỉnh Lâm Đồng", "Lâm Đồng" },
                    { 108, null, null, "Bách hóa xanh", "TP. Hồ Chí Minh" },
                    { 109, null, null, "WinMart", "Hà Nội" }
                });
        }
    }
}

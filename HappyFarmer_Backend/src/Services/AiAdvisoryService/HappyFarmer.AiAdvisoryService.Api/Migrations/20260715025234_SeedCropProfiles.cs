using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.AiAdvisoryService.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedCropProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CropProfiles",
                columns: new[] { "Id", "CropTypeCode", "CropNameVi", "AvgDaysToHarvest", "IdealTempMin", "IdealTempMax", "IdealRainfallMm", "Notes" },
                values: new object[,]
                {
                    {
                        1, "rice", "Lúa", 100, 25.0, 32.0, 200.0,
                        "Số liệu trung bình cho giống lúa phổ biến ở ĐBSCL — thời gian sinh trưởng thực " +
                        "tế dao động 90-130 ngày tùy giống (ngắn/dài ngày) và vùng miền."
                    },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "CropProfiles", keyColumn: "Id", keyValue: 1);
        }
    }
}

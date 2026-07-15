using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.AiAdvisoryService.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCropProfilesAndHarvestPredictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CropProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CropTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CropNameVi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AvgDaysToHarvest = table.Column<int>(type: "int", nullable: false),
                    IdealTempMin = table.Column<double>(type: "float", nullable: false),
                    IdealTempMax = table.Column<double>(type: "float", nullable: false),
                    IdealRainfallMm = table.Column<double>(type: "float", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HarvestPredictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    CropType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlantingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecommendedStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RecommendedEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ConfidenceLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReasoningText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RiskFactorsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeatherSummaryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsedVerifiedCropProfile = table.Column<bool>(type: "bit", nullable: false),
                    WeatherDataIncluded = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HarvestPredictions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CropProfiles_CropNameVi",
                table: "CropProfiles",
                column: "CropNameVi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HarvestPredictions_FarmerId",
                table: "HarvestPredictions",
                column: "FarmerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CropProfiles");

            migrationBuilder.DropTable(
                name: "HarvestPredictions");
        }
    }
}

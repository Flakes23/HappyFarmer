using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.AiAdvisoryService.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDiseaseDetections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiseaseDetections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CropTypeHint = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsHealthy = table.Column<bool>(type: "bit", nullable: false),
                    IdentifiedCropType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiseaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TreatmentOrganicJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TreatmentChemicalJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreventionTipsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecommendedActionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseaseDetections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiseaseDetections_FarmerId",
                table: "DiseaseDetections",
                column: "FarmerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiseaseDetections");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.AiAdvisoryService.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMessageCardsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardsJson",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardsJson",
                table: "ChatMessages");
        }
    }
}

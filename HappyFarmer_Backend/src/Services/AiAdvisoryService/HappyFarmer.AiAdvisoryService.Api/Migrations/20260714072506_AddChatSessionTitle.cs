using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyFarmer.AiAdvisoryService.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSessionTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ChatSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "ChatSessions");
        }
    }
}

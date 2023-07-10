using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations
{
    /// <inheritdoc />
    public partial class addRequestingMessageID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrentPath",
                table: "TelegramUsers",
                newName: "TempData");

            migrationBuilder.AddColumn<int>(
                name: "RequestingMessageID",
                table: "TelegramUsers",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestingMessageID",
                table: "TelegramUsers");

            migrationBuilder.RenameColumn(
                name: "TempData",
                table: "TelegramUsers",
                newName: "CurrentPath");
        }
    }
}

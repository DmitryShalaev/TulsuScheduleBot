using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class rmRequestingMessageID : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "RequestingMessageID",
                table: "TelegramUsersTmp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<int>(
                name: "RequestingMessageID",
                table: "TelegramUsersTmp",
                type: "integer",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class AddLogInfo : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdate",
                table: "TelegramUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TodayRequests",
                table: "TelegramUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TotalRequests",
                table: "TelegramUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "LastUpdate",
                table: "TelegramUsers");

            migrationBuilder.DropColumn(
                name: "TodayRequests",
                table: "TelegramUsers");

            migrationBuilder.DropColumn(
                name: "TotalRequests",
                table: "TelegramUsers");
        }
    }
}

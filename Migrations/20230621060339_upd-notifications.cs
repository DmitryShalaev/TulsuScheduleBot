using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations
{
    /// <inheritdoc />
    public partial class updnotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TelegramUsers_Notifications_NotificationsID",
                table: "TelegramUsers");

            migrationBuilder.AlterColumn<long>(
                name: "NotificationsID",
                table: "TelegramUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_TelegramUsers_Notifications_NotificationsID",
                table: "TelegramUsers",
                column: "NotificationsID",
                principalTable: "Notifications",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TelegramUsers_Notifications_NotificationsID",
                table: "TelegramUsers");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Notifications");

            migrationBuilder.AlterColumn<long>(
                name: "NotificationsID",
                table: "TelegramUsers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_TelegramUsers_Notifications_NotificationsID",
                table: "TelegramUsers",
                column: "NotificationsID",
                principalTable: "Notifications",
                principalColumn: "ID");
        }
    }
}

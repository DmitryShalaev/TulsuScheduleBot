using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations
{
    /// <inheritdoc />
    public partial class upd_TelegramUserForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_TelegramUsers_OwnerID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleProfile_TelegramUsers_OwnerID",
                table: "ScheduleProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_TelegramUsers_Notifications_NotificationsID",
                table: "TelegramUsers");

            migrationBuilder.DropIndex(
                name: "IX_TelegramUsers_NotificationsID",
                table: "TelegramUsers");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleProfile_OwnerID",
                table: "ScheduleProfile");

            migrationBuilder.DropColumn(
                name: "NotificationsID",
                table: "TelegramUsers");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerID",
                table: "ScheduleProfile",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "OwnerID",
                table: "Notifications",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_TelegramUsers_OwnerID",
                table: "Notifications",
                column: "OwnerID",
                principalTable: "TelegramUsers",
                principalColumn: "ChatID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_TelegramUsers_OwnerID",
                table: "Notifications");

            migrationBuilder.AddColumn<long>(
                name: "NotificationsID",
                table: "TelegramUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "OwnerID",
                table: "ScheduleProfile",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerID",
                table: "Notifications",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_NotificationsID",
                table: "TelegramUsers",
                column: "NotificationsID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleProfile_OwnerID",
                table: "ScheduleProfile",
                column: "OwnerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_TelegramUsers_OwnerID",
                table: "Notifications",
                column: "OwnerID",
                principalTable: "TelegramUsers",
                principalColumn: "ChatID");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleProfile_TelegramUsers_OwnerID",
                table: "ScheduleProfile",
                column: "OwnerID",
                principalTable: "TelegramUsers",
                principalColumn: "ChatID");

            migrationBuilder.AddForeignKey(
                name: "FK_TelegramUsers_Notifications_NotificationsID",
                table: "TelegramUsers",
                column: "NotificationsID",
                principalTable: "Notifications",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

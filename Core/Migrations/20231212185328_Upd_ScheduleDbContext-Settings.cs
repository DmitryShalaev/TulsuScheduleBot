using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class Upd_ScheduleDbContextSettings : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_TelegramUsers_OwnerID",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "Settings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Settings",
                table: "Settings",
                column: "OwnerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_TelegramUsers_OwnerID",
                table: "Settings",
                column: "OwnerID",
                principalTable: "TelegramUsers",
                principalColumn: "ChatID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Settings_TelegramUsers_OwnerID",
                table: "Settings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Settings",
                table: "Settings");

            migrationBuilder.RenameTable(
                name: "Settings",
                newName: "Notifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "OwnerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_TelegramUsers_OwnerID",
                table: "Notifications",
                column: "OwnerID",
                principalTable: "TelegramUsers",
                principalColumn: "ChatID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

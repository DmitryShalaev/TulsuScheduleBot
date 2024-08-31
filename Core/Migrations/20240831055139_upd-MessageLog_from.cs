using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class updMessageLog_from : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageLog_TelegramUsers_TelegramUserChatID",
                table: "MessageLog");

            migrationBuilder.RenameColumn(
                name: "TelegramUserChatID",
                table: "MessageLog",
                newName: "From");

            migrationBuilder.RenameIndex(
                name: "IX_MessageLog_TelegramUserChatID",
                table: "MessageLog",
                newName: "IX_MessageLog_From");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLog_TelegramUsers_From",
                table: "MessageLog",
                column: "From",
                principalTable: "TelegramUsers",
                principalColumn: "ChatID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageLog_TelegramUsers_From",
                table: "MessageLog");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "MessageLog",
                newName: "TelegramUserChatID");

            migrationBuilder.RenameIndex(
                name: "IX_MessageLog_From",
                table: "MessageLog",
                newName: "IX_MessageLog_TelegramUserChatID");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLog_TelegramUsers_TelegramUserChatID",
                table: "MessageLog",
                column: "TelegramUserChatID",
                principalTable: "TelegramUsers",
                principalColumn: "ChatID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

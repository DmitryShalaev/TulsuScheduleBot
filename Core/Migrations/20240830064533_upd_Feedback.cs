using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class upd_Feedback : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_TelegramUsers_TelegramUserChatID",
                table: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "TelegramUserChatID",
                table: "Feedbacks",
                newName: "From");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_TelegramUserChatID",
                table: "Feedbacks",
                newName: "IX_Feedbacks_From");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_TelegramUsers_From",
                table: "Feedbacks",
                column: "From",
                principalTable: "TelegramUsers",
                principalColumn: "ChatID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_TelegramUsers_From",
                table: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "Feedbacks",
                newName: "TelegramUserChatID");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_From",
                table: "Feedbacks",
                newName: "IX_Feedbacks_TelegramUserChatID");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_TelegramUsers_TelegramUserChatID",
                table: "Feedbacks",
                column: "TelegramUserChatID",
                principalTable: "TelegramUsers",
                principalColumn: "ChatID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

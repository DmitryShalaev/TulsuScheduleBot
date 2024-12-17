using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ScheduleBot.Migrations
{
    /// <inheritdoc />
    public partial class upd_db_UpdateTypeDTO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageLog_UpdateTypes_UpdateType",
                table: "MessageLog");

            migrationBuilder.DropTable(
                name: "UpdateTypes");

            migrationBuilder.DropIndex(
                name: "IX_MessageLog_UpdateType",
                table: "MessageLog");

            migrationBuilder.DropColumn(
                name: "UpdateType",
                table: "MessageLog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UpdateType",
                table: "MessageLog",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UpdateTypes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateTypes", x => x.ID);
                });

            migrationBuilder.InsertData(
                table: "UpdateTypes",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { 0, "Unknown" },
                    { 1, "Message" },
                    { 2, "InlineQuery" },
                    { 3, "ChosenInlineResult" },
                    { 4, "CallbackQuery" },
                    { 5, "EditedMessage" },
                    { 6, "ChannelPost" },
                    { 7, "EditedChannelPost" },
                    { 8, "ShippingQuery" },
                    { 9, "PreCheckoutQuery" },
                    { 10, "Poll" },
                    { 11, "PollAnswer" },
                    { 12, "MyChatMember" },
                    { 13, "ChatMember" },
                    { 14, "ChatJoinRequest" },
                    { 15, "MessageReaction" },
                    { 16, "MessageReactionCount" },
                    { 17, "ChatBoost" },
                    { 18, "RemovedChatBoost" },
                    { 19, "BusinessConnection" },
                    { 20, "BusinessMessage" },
                    { 21, "EditedBusinessMessage" },
                    { 22, "DeletedBusinessMessages" },
                    { 23, "PurchasedPaidMedia" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_UpdateType",
                table: "MessageLog",
                column: "UpdateType");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLog_UpdateTypes_UpdateType",
                table: "MessageLog",
                column: "UpdateType",
                principalTable: "UpdateTypes",
                principalColumn: "ID");
        }
    }
}

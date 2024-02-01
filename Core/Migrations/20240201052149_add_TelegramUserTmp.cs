using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class add_TelegramUserTmp : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_TelegramUsers_Modes_Mode",
                table: "TelegramUsers");

            migrationBuilder.DropIndex(
                name: "IX_TelegramUsers_Mode",
                table: "TelegramUsers");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "TelegramUsers");

            migrationBuilder.DropColumn(
                name: "RequestingMessageID",
                table: "TelegramUsers");

            migrationBuilder.DropColumn(
                name: "TempData",
                table: "TelegramUsers");

            migrationBuilder.CreateTable(
                name: "TelegramUserTmp",
                columns: table => new {
                    OwnerID = table.Column<long>(type: "bigint", nullable: false),
                    Mode = table.Column<byte>(type: "smallint", nullable: false),
                    RequestingMessageID = table.Column<int>(type: "integer", nullable: true),
                    TmpData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_TelegramUserTmp", x => x.OwnerID);
                    table.ForeignKey(
                        name: "FK_TelegramUserTmp_Modes_Mode",
                        column: x => x.Mode,
                        principalTable: "Modes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramUserTmp_TelegramUsers_OwnerID",
                        column: x => x.OwnerID,
                        principalTable: "TelegramUsers",
                        principalColumn: "ChatID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUserTmp_Mode",
                table: "TelegramUserTmp",
                column: "Mode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "TelegramUserTmp");

            migrationBuilder.AddColumn<byte>(
                name: "Mode",
                table: "TelegramUsers",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "RequestingMessageID",
                table: "TelegramUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TempData",
                table: "TelegramUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_Mode",
                table: "TelegramUsers",
                column: "Mode");

            migrationBuilder.AddForeignKey(
                name: "FK_TelegramUsers_Modes_Mode",
                table: "TelegramUsers",
                column: "Mode",
                principalTable: "Modes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

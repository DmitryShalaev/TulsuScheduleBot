using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class AddTemporaryAddition : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Types",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "ChatId",
                table: "TelegramUsers",
                newName: "ChatID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Disciplines",
                newName: "ID");

            migrationBuilder.AddColumn<byte>(
                name: "Mode",
                table: "TelegramUsers",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "Modes",
                columns: table => new {
                    ID = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Modes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TemporaryAddition",
                columns: table => new {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User = table.Column<long>(type: "bigint", nullable: false),
                    AddDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Counter = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    LectureHall = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_TemporaryAddition", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TemporaryAddition_TelegramUsers_User",
                        column: x => x.User,
                        principalTable: "TelegramUsers",
                        principalColumn: "ChatID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Modes",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Default" },
                    { (byte)1, "AddingDiscipline" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_Mode",
                table: "TelegramUsers",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_Class",
                table: "Disciplines",
                column: "Class");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedDisciplines_Class",
                table: "CompletedDisciplines",
                column: "Class");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryAddition_User",
                table: "TemporaryAddition",
                column: "User");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedDisciplines_Types_Class",
                table: "CompletedDisciplines",
                column: "Class",
                principalTable: "Types",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Disciplines_Types_Class",
                table: "Disciplines",
                column: "Class",
                principalTable: "Types",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TelegramUsers_Modes_Mode",
                table: "TelegramUsers",
                column: "Mode",
                principalTable: "Modes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedDisciplines_Types_Class",
                table: "CompletedDisciplines");

            migrationBuilder.DropForeignKey(
                name: "FK_Disciplines_Types_Class",
                table: "Disciplines");

            migrationBuilder.DropForeignKey(
                name: "FK_TelegramUsers_Modes_Mode",
                table: "TelegramUsers");

            migrationBuilder.DropTable(
                name: "Modes");

            migrationBuilder.DropTable(
                name: "TemporaryAddition");

            migrationBuilder.DropIndex(
                name: "IX_TelegramUsers_Mode",
                table: "TelegramUsers");

            migrationBuilder.DropIndex(
                name: "IX_Disciplines_Class",
                table: "Disciplines");

            migrationBuilder.DropIndex(
                name: "IX_CompletedDisciplines_Class",
                table: "CompletedDisciplines");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "TelegramUsers");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Types",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ChatID",
                table: "TelegramUsers",
                newName: "ChatId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Disciplines",
                newName: "Id");
        }
    }
}

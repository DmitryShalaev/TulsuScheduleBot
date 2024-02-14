using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class add_DeletedDisciplines : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "TelegramUserTmp");

            migrationBuilder.CreateTable(
                name: "DeletedDisciplines",
                columns: table => new {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    LectureHall = table.Column<string>(type: "text", nullable: false),
                    Subgroup = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DeleteDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Group = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_DeletedDisciplines", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DeletedDisciplines_Classes_Class",
                        column: x => x.Class,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeletedDisciplines_GroupLastUpdate_Group",
                        column: x => x.Group,
                        principalTable: "GroupLastUpdate",
                        principalColumn: "Group",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeletedDisciplines_TeacherLastUpdate_Lecturer",
                        column: x => x.Lecturer,
                        principalTable: "TeacherLastUpdate",
                        principalColumn: "Teacher");
                });

            migrationBuilder.CreateTable(
                name: "TelegramUsersTmp",
                columns: table => new {
                    OwnerID = table.Column<long>(type: "bigint", nullable: false),
                    Mode = table.Column<byte>(type: "smallint", nullable: false),
                    RequestingMessageID = table.Column<int>(type: "integer", nullable: true),
                    TmpData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_TelegramUsersTmp", x => x.OwnerID);
                    table.ForeignKey(
                        name: "FK_TelegramUsersTmp_Modes_Mode",
                        column: x => x.Mode,
                        principalTable: "Modes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramUsersTmp_TelegramUsers_OwnerID",
                        column: x => x.OwnerID,
                        principalTable: "TelegramUsers",
                        principalColumn: "ChatID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeletedDisciplines_Class",
                table: "DeletedDisciplines",
                column: "Class");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedDisciplines_Date",
                table: "DeletedDisciplines",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedDisciplines_Group",
                table: "DeletedDisciplines",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedDisciplines_Lecturer",
                table: "DeletedDisciplines",
                column: "Lecturer");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsersTmp_Mode",
                table: "TelegramUsersTmp",
                column: "Mode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "DeletedDisciplines");

            migrationBuilder.DropTable(
                name: "TelegramUsersTmp");

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
    }
}

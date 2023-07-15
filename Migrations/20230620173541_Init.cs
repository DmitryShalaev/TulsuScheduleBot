using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScheduleBot.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    ID = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GroupLastUpdate",
                columns: table => new
                {
                    Group = table.Column<string>(type: "text", nullable: false),
                    Update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupLastUpdate", x => x.Group);
                });

            migrationBuilder.CreateTable(
                name: "Modes",
                columns: table => new
                {
                    ID = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Progresses",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Discipline = table.Column<string>(type: "text", nullable: false),
                    MarkTitle = table.Column<string>(type: "text", nullable: true),
                    Mark = table.Column<int>(type: "integer", nullable: true),
                    Term = table.Column<int>(type: "integer", nullable: false),
                    StudentID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Progresses", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StudentIDLastUpdate",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "text", nullable: false),
                    Update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentIDLastUpdate", x => x.StudentID);
                });

            migrationBuilder.CreateTable(
                name: "Disciplines",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    LectureHall = table.Column<string>(type: "text", nullable: false),
                    Subgroup = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Group = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplines", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Disciplines_Classes_Class",
                        column: x => x.Class,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletedDisciplines",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    Subgroup = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    ScheduleProfileGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedDisciplines", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CompletedDisciplines_Classes_Class",
                        column: x => x.Class,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomDiscipline",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Counter = table.Column<int>(type: "integer", nullable: false),
                    IsAdded = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    LectureHall = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ScheduleProfileGuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomDiscipline", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MessageLog",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TelegramUserChatID = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageLog", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DNDStart = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    DNDStop = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Days = table.Column<int>(type: "integer", nullable: false),
                    OwnerID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleProfile",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerID = table.Column<long>(type: "bigint", nullable: true),
                    Group = table.Column<string>(type: "text", nullable: true),
                    StudentID = table.Column<string>(type: "text", nullable: true),
                    LastAppeal = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleProfile", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new
                {
                    ChatID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    CurrentPath = table.Column<string>(type: "text", nullable: true),
                    LastAppeal = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalRequests = table.Column<long>(type: "bigint", nullable: false),
                    TodayRequests = table.Column<long>(type: "bigint", nullable: false),
                    ScheduleProfileGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationsID = table.Column<long>(type: "bigint", nullable: true),
                    Mode = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUsers", x => x.ChatID);
                    table.ForeignKey(
                        name: "FK_TelegramUsers_Modes_Mode",
                        column: x => x.Mode,
                        principalTable: "Modes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramUsers_Notifications_NotificationsID",
                        column: x => x.NotificationsID,
                        principalTable: "Notifications",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TelegramUsers_ScheduleProfile_ScheduleProfileGuid",
                        column: x => x.ScheduleProfileGuid,
                        principalTable: "ScheduleProfile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { (byte)0, "all" },
                    { (byte)1, "lab" },
                    { (byte)2, "practice" },
                    { (byte)3, "lecture" },
                    { (byte)4, "other" },
                    { (byte)5, "custom" }
                });

            migrationBuilder.InsertData(
                table: "Modes",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Default" },
                    { (byte)1, "AddingDiscipline" },
                    { (byte)2, "GroupСhange" },
                    { (byte)3, "StudentIDСhange" },
                    { (byte)4, "ResetProfileLink" },
                    { (byte)5, "CustomEditName" },
                    { (byte)6, "CustomEditLecturer" },
                    { (byte)7, "CustomEditType" },
                    { (byte)8, "CustomEditLectureHall" },
                    { (byte)9, "CustomEditStartTime" },
                    { (byte)10, "CustomEditEndTime" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedDisciplines_Class",
                table: "CompletedDisciplines",
                column: "Class");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedDisciplines_ScheduleProfileGuid",
                table: "CompletedDisciplines",
                column: "ScheduleProfileGuid");

            migrationBuilder.CreateIndex(
                name: "IX_CustomDiscipline_ScheduleProfileGuid",
                table: "CustomDiscipline",
                column: "ScheduleProfileGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_Class",
                table: "Disciplines",
                column: "Class");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_TelegramUserChatID",
                table: "MessageLog",
                column: "TelegramUserChatID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_OwnerID",
                table: "Notifications",
                column: "OwnerID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleProfile_OwnerID",
                table: "ScheduleProfile",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_Mode",
                table: "TelegramUsers",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_NotificationsID",
                table: "TelegramUsers",
                column: "NotificationsID");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_ScheduleProfileGuid",
                table: "TelegramUsers",
                column: "ScheduleProfileGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedDisciplines_ScheduleProfile_ScheduleProfileGuid",
                table: "CompletedDisciplines",
                column: "ScheduleProfileGuid",
                principalTable: "ScheduleProfile",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomDiscipline_ScheduleProfile_ScheduleProfileGuid",
                table: "CustomDiscipline",
                column: "ScheduleProfileGuid",
                principalTable: "ScheduleProfile",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLog_TelegramUsers_TelegramUserChatID",
                table: "MessageLog",
                column: "TelegramUserChatID",
                principalTable: "TelegramUsers",
                principalColumn: "ChatID",
                onDelete: ReferentialAction.Cascade);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TelegramUsers_ScheduleProfile_ScheduleProfileGuid",
                table: "TelegramUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_TelegramUsers_OwnerID",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "CompletedDisciplines");

            migrationBuilder.DropTable(
                name: "CustomDiscipline");

            migrationBuilder.DropTable(
                name: "Disciplines");

            migrationBuilder.DropTable(
                name: "GroupLastUpdate");

            migrationBuilder.DropTable(
                name: "MessageLog");

            migrationBuilder.DropTable(
                name: "Progresses");

            migrationBuilder.DropTable(
                name: "StudentIDLastUpdate");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "ScheduleProfile");

            migrationBuilder.DropTable(
                name: "TelegramUsers");

            migrationBuilder.DropTable(
                name: "Modes");

            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}

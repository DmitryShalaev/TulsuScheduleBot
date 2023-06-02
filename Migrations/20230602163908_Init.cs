using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class Init : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new {
                    ID = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Classes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GroupLastUpdate",
                columns: table => new {
                    Group = table.Column<string>(type: "text", nullable: false),
                    Update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_GroupLastUpdate", x => x.Group);
                });

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
                name: "Progresses",
                columns: table => new {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Discipline = table.Column<string>(type: "text", nullable: false),
                    MarkTitle = table.Column<string>(type: "text", nullable: true),
                    Mark = table.Column<int>(type: "integer", nullable: true),
                    Term = table.Column<int>(type: "integer", nullable: false),
                    StudentID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Progresses", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleProfile",
                columns: table => new {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerID = table.Column<long>(type: "bigint", nullable: false),
                    Group = table.Column<string>(type: "text", nullable: true),
                    StudentID = table.Column<string>(type: "text", nullable: true),
                    LastAppeal = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ScheduleProfile", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StudentIDLastUpdate",
                columns: table => new {
                    StudentID = table.Column<string>(type: "text", nullable: false),
                    Update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_StudentIDLastUpdate", x => x.StudentID);
                });

            migrationBuilder.CreateTable(
                name: "Disciplines",
                columns: table => new {
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
                constraints: table => {
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
                columns: table => new {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    Subgroup = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    ScheduleProfileGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_CompletedDisciplines", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CompletedDisciplines_Classes_Class",
                        column: x => x.Class,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletedDisciplines_ScheduleProfile_ScheduleProfileGuid",
                        column: x => x.ScheduleProfileGuid,
                        principalTable: "ScheduleProfile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomDiscipline",
                columns: table => new {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    LectureHall = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ScheduleProfileGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_CustomDiscipline", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CustomDiscipline_Classes_Class",
                        column: x => x.Class,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomDiscipline_ScheduleProfile_ScheduleProfileGuid",
                        column: x => x.ScheduleProfileGuid,
                        principalTable: "ScheduleProfile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new {
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
                    Mode = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_TelegramUsers", x => x.ChatID);
                    table.ForeignKey(
                        name: "FK_TelegramUsers_Modes_Mode",
                        column: x => x.Mode,
                        principalTable: "Modes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramUsers_ScheduleProfile_ScheduleProfileGuid",
                        column: x => x.ScheduleProfileGuid,
                        principalTable: "ScheduleProfile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
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
                    Type = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
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
                table: "Classes",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { (byte)0, "all" },
                    { (byte)1, "lab" },
                    { (byte)2, "practice" },
                    { (byte)3, "lecture" },
                    { (byte)4, "other" }
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
                name: "IX_CustomDiscipline_Class",
                table: "CustomDiscipline",
                column: "Class");

            migrationBuilder.CreateIndex(
                name: "IX_CustomDiscipline_ScheduleProfileGuid",
                table: "CustomDiscipline",
                column: "ScheduleProfileGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_Class",
                table: "Disciplines",
                column: "Class");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_Mode",
                table: "TelegramUsers",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_ScheduleProfileGuid",
                table: "TelegramUsers",
                column: "ScheduleProfileGuid");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryAddition_User",
                table: "TemporaryAddition",
                column: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "CompletedDisciplines");

            migrationBuilder.DropTable(
                name: "CustomDiscipline");

            migrationBuilder.DropTable(
                name: "Disciplines");

            migrationBuilder.DropTable(
                name: "GroupLastUpdate");

            migrationBuilder.DropTable(
                name: "Progresses");

            migrationBuilder.DropTable(
                name: "StudentIDLastUpdate");

            migrationBuilder.DropTable(
                name: "TemporaryAddition");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "TelegramUsers");

            migrationBuilder.DropTable(
                name: "Modes");

            migrationBuilder.DropTable(
                name: "ScheduleProfile");
        }
    }
}

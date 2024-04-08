using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class addTeacher : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "TeacherLastUpdate",
                columns: table => new {
                    Teacher = table.Column<string>(type: "text", nullable: false),
                    Update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_TeacherLastUpdate", x => x.Teacher);
                });

            migrationBuilder.CreateTable(
                name: "TeacherWorkSchedule",
                columns: table => new {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lecturer = table.Column<string>(type: "text", nullable: true),
                    LectureHall = table.Column<string>(type: "text", nullable: false),
                    Groups = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_TeacherWorkSchedule", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TeacherWorkSchedule_Classes_Class",
                        column: x => x.Class,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Modes",
                columns: ["ID", "Name"],
                values: [(byte)12, "TeachersWorkSchedule"]);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherWorkSchedule_Class",
                table: "TeacherWorkSchedule",
                column: "Class");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "TeacherLastUpdate");

            migrationBuilder.DropTable(
                name: "TeacherWorkSchedule");

            migrationBuilder.DeleteData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)12);
        }
    }
}

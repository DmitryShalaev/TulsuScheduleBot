using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class updClassroomWorkScheduleclass : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "ClassroomWorkSchedule",
                columns: table => new {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lecturer = table.Column<string>(type: "text", nullable: false),
                    LectureHall = table.Column<string>(type: "text", nullable: false),
                    Groups = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ClassroomWorkSchedule", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ClassroomWorkSchedule_Classes_Class",
                        column: x => x.Class,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassroomWorkSchedule_ClassroomLastUpdate_LectureHall",
                        column: x => x.LectureHall,
                        principalTable: "ClassroomLastUpdate",
                        principalColumn: "Classroom",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassroomWorkSchedule_TeacherLastUpdate_Lecturer",
                        column: x => x.Lecturer,
                        principalTable: "TeacherLastUpdate",
                        principalColumn: "Teacher",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomWorkSchedule_Class",
                table: "ClassroomWorkSchedule",
                column: "Class");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomWorkSchedule_LectureHall",
                table: "ClassroomWorkSchedule",
                column: "LectureHall");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomWorkSchedule_Lecturer",
                table: "ClassroomWorkSchedule",
                column: "Lecturer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "ClassroomWorkSchedule");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class addClassroomWorkSchedule : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "ClassroomLastUpdateClassroom",
                table: "TeacherWorkSchedule",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherWorkSchedule_ClassroomLastUpdateClassroom",
                table: "TeacherWorkSchedule",
                column: "ClassroomLastUpdateClassroom");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherWorkSchedule_ClassroomLastUpdate_ClassroomLastUpdate~",
                table: "TeacherWorkSchedule",
                column: "ClassroomLastUpdateClassroom",
                principalTable: "ClassroomLastUpdate",
                principalColumn: "Classroom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherWorkSchedule_ClassroomLastUpdate_ClassroomLastUpdate~",
                table: "TeacherWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_TeacherWorkSchedule_ClassroomLastUpdateClassroom",
                table: "TeacherWorkSchedule");

            migrationBuilder.DropColumn(
                name: "ClassroomLastUpdateClassroom",
                table: "TeacherWorkSchedule");
        }
    }
}

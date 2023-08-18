using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class upd_LecturerTeacherWorkSchedule : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateIndex(
                name: "IX_TeacherWorkSchedule_Lecturer",
                table: "TeacherWorkSchedule",
                column: "Lecturer");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherWorkSchedule_TeacherLastUpdate_Lecturer",
                table: "TeacherWorkSchedule",
                column: "Lecturer",
                principalTable: "TeacherLastUpdate",
                principalColumn: "Teacher");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherWorkSchedule_TeacherLastUpdate_Lecturer",
                table: "TeacherWorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_TeacherWorkSchedule_Lecturer",
                table: "TeacherWorkSchedule");
        }
    }
}

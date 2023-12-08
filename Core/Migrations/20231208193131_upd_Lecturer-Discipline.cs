using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class Upd_LecturerDiscipline : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_Lecturer",
                table: "Disciplines",
                column: "Lecturer");

            migrationBuilder.AddForeignKey(
                name: "FK_Disciplines_TeacherLastUpdate_Lecturer",
                table: "Disciplines",
                column: "Lecturer",
                principalTable: "TeacherLastUpdate",
                principalColumn: "Teacher");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Disciplines_TeacherLastUpdate_Lecturer",
                table: "Disciplines");

            migrationBuilder.DropIndex(
                name: "IX_Disciplines_Lecturer",
                table: "Disciplines");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class updDiscipline : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_LectureHall",
                table: "Disciplines",
                column: "LectureHall");

            migrationBuilder.AddForeignKey(
                name: "FK_Disciplines_ClassroomLastUpdate_LectureHall",
                table: "Disciplines",
                column: "LectureHall",
                principalTable: "ClassroomLastUpdate",
                principalColumn: "Classroom",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Disciplines_ClassroomLastUpdate_LectureHall",
                table: "Disciplines");

            migrationBuilder.DropIndex(
                name: "IX_Disciplines_LectureHall",
                table: "Disciplines");
        }
    }
}

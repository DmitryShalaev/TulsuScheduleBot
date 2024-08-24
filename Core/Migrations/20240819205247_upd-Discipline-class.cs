using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class updDisciplineclass : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Disciplines_ClassroomLastUpdate_LectureHall",
                table: "Disciplines");

            migrationBuilder.AlterColumn<string>(
                name: "LectureHall",
                table: "Disciplines",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Disciplines_ClassroomLastUpdate_LectureHall",
                table: "Disciplines",
                column: "LectureHall",
                principalTable: "ClassroomLastUpdate",
                principalColumn: "Classroom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Disciplines_ClassroomLastUpdate_LectureHall",
                table: "Disciplines");

            migrationBuilder.AlterColumn<string>(
                name: "LectureHall",
                table: "Disciplines",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Disciplines_ClassroomLastUpdate_LectureHall",
                table: "Disciplines",
                column: "LectureHall",
                principalTable: "ClassroomLastUpdate",
                principalColumn: "Classroom",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

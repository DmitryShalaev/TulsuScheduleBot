using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class updClassroomWorkScheduleLecturer : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassroomWorkSchedule_TeacherLastUpdate_Lecturer",
                table: "ClassroomWorkSchedule");

            migrationBuilder.AlterColumn<string>(
                name: "Lecturer",
                table: "ClassroomWorkSchedule",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassroomWorkSchedule_TeacherLastUpdate_Lecturer",
                table: "ClassroomWorkSchedule",
                column: "Lecturer",
                principalTable: "TeacherLastUpdate",
                principalColumn: "Teacher");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassroomWorkSchedule_TeacherLastUpdate_Lecturer",
                table: "ClassroomWorkSchedule");

            migrationBuilder.AlterColumn<string>(
                name: "Lecturer",
                table: "ClassroomWorkSchedule",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassroomWorkSchedule_TeacherLastUpdate_Lecturer",
                table: "ClassroomWorkSchedule",
                column: "Lecturer",
                principalTable: "TeacherLastUpdate",
                principalColumn: "Teacher",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

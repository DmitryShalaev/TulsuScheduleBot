using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations
{
    /// <inheritdoc />
    public partial class upd_TeacherWorkScheduleTeacherLastUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherWorkSchedule_TeacherLastUpdate_Lecturer",
                table: "TeacherWorkSchedule");

            migrationBuilder.AlterColumn<string>(
                name: "Lecturer",
                table: "TeacherWorkSchedule",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherWorkSchedule_TeacherLastUpdate_Lecturer",
                table: "TeacherWorkSchedule",
                column: "Lecturer",
                principalTable: "TeacherLastUpdate",
                principalColumn: "Teacher",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherWorkSchedule_TeacherLastUpdate_Lecturer",
                table: "TeacherWorkSchedule");

            migrationBuilder.AlterColumn<string>(
                name: "Lecturer",
                table: "TeacherWorkSchedule",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherWorkSchedule_TeacherLastUpdate_Lecturer",
                table: "TeacherWorkSchedule",
                column: "Lecturer",
                principalTable: "TeacherLastUpdate",
                principalColumn: "Teacher");
        }
    }
}

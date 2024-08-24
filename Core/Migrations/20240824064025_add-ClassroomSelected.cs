using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class addClassroomSelected : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.UpdateData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)14,
                column: "Name",
                value: "ClassroomSchedule");

            migrationBuilder.UpdateData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)15,
                column: "Name",
                value: "ClassroomSelected");

            migrationBuilder.InsertData(
                table: "Modes",
                columns: new[] { "ID", "Name" },
                values: new object[] { (byte)16, "Feedback" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DeleteData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)16);

            migrationBuilder.UpdateData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)14,
                column: "Name",
                value: "ClassroomsSchedule");

            migrationBuilder.UpdateData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)15,
                column: "Name",
                value: "Feedback");
        }
    }
}

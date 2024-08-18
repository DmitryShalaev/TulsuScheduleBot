using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class addModeClassroomsSchedule : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.UpdateData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)14,
                column: "Name",
                value: "ClassroomsSchedule");

            migrationBuilder.InsertData(
                table: "Modes",
                columns: new[] { "ID", "Name" },
                values: new object[] { (byte)15, "Feedback" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DeleteData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)15);

            migrationBuilder.UpdateData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)14,
                column: "Name",
                value: "Feedback");
        }
    }
}

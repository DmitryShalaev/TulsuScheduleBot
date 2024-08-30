using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class addMode_Messenger : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.UpdateData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)17,
                column: "Name",
                value: "Messenger");

            migrationBuilder.InsertData(
                table: "Modes",
                columns: new[] { "ID", "Name" },
                values: new object[] { (byte)18, "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DeleteData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)18);

            migrationBuilder.UpdateData(
                table: "Modes",
                keyColumn: "ID",
                keyValue: (byte)17,
                column: "Name",
                value: "Admin");
        }
    }
}

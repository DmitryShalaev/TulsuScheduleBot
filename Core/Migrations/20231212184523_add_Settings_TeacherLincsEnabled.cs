using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class add_Settings_TeacherLincsEnabled : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "NotificationIsEnabled",
                table: "Notifications",
                newName: "TeacherLincsEnabled");

            migrationBuilder.AddColumn<bool>(
                name: "NotificationEnabled",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "NotificationEnabled",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "TeacherLincsEnabled",
                table: "Notifications",
                newName: "NotificationIsEnabled");
        }
    }
}

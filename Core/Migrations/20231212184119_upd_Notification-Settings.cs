using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class upd_NotificationSettings : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "Notifications",
                newName: "NotificationIsEnabled");

            migrationBuilder.RenameColumn(
                name: "Days",
                table: "Notifications",
                newName: "NotificationDays");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "NotificationIsEnabled",
                table: "Notifications",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "NotificationDays",
                table: "Notifications",
                newName: "Days");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class addMessageLog_Request : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "Request",
                table: "MessageLog",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "Request",
                table: "MessageLog");
        }
    }
}


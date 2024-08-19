using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class addClassroomLastUpdate : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "ClassroomLastUpdate",
                columns: table => new {
                    Classroom = table.Column<string>(type: "text", nullable: false),
                    Update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAttempt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ClassroomLastUpdate", x => x.Classroom);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "ClassroomLastUpdate");
        }
    }
}

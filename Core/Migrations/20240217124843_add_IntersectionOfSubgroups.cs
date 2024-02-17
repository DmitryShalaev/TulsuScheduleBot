using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class add_IntersectionOfSubgroups : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "IntersectionMark",
                table: "Disciplines",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IntersectionOfSubgroups",
                columns: table => new {
                    Group = table.Column<string>(type: "text", nullable: false),
                    IntersectionWith = table.Column<string>(type: "text", nullable: false),
                    Mark = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_IntersectionOfSubgroups", x => x.Group);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntersectionOfSubgroups_IntersectionWith",
                table: "IntersectionOfSubgroups",
                column: "IntersectionWith",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "IntersectionOfSubgroups");

            migrationBuilder.DropColumn(
                name: "IntersectionMark",
                table: "Disciplines");
        }
    }
}

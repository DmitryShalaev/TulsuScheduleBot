using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class upd_Group_in_Discipline : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_Group",
                table: "Disciplines",
                column: "Group");

            migrationBuilder.AddForeignKey(
                name: "FK_Disciplines_GroupLastUpdate_Group",
                table: "Disciplines",
                column: "Group",
                principalTable: "GroupLastUpdate",
                principalColumn: "Group",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Disciplines_GroupLastUpdate_Group",
                table: "Disciplines");

            migrationBuilder.DropIndex(
                name: "IX_Disciplines_Group",
                table: "Disciplines");
        }
    }
}

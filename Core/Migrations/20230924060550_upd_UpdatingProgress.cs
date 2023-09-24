using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class upd_UpdatingProgress : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateIndex(
                name: "IX_Progresses_StudentID",
                table: "Progresses",
                column: "StudentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_StudentIDLastUpdate_StudentID",
                table: "Progresses",
                column: "StudentID",
                principalTable: "StudentIDLastUpdate",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_StudentIDLastUpdate_StudentID",
                table: "Progresses");

            migrationBuilder.DropIndex(
                name: "IX_Progresses_StudentID",
                table: "Progresses");
        }
    }
}

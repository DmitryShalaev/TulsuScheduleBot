using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class updScheduleProfile : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateIndex(
                name: "IX_ScheduleProfile_Group",
                table: "ScheduleProfile",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleProfile_StudentID",
                table: "ScheduleProfile",
                column: "StudentID");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleProfile_GroupLastUpdate_Group",
                table: "ScheduleProfile",
                column: "Group",
                principalTable: "GroupLastUpdate",
                principalColumn: "Group");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleProfile_StudentIDLastUpdate_StudentID",
                table: "ScheduleProfile",
                column: "StudentID",
                principalTable: "StudentIDLastUpdate",
                principalColumn: "StudentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleProfile_GroupLastUpdate_Group",
                table: "ScheduleProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleProfile_StudentIDLastUpdate_StudentID",
                table: "ScheduleProfile");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleProfile_Group",
                table: "ScheduleProfile");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleProfile_StudentID",
                table: "ScheduleProfile");
        }
    }
}

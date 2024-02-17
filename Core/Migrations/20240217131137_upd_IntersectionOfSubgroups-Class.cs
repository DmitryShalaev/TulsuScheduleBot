using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class upd_IntersectionOfSubgroupsClass : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<byte>(
                name: "Class",
                table: "IntersectionOfSubgroups",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_IntersectionOfSubgroups_Class",
                table: "IntersectionOfSubgroups",
                column: "Class");

            migrationBuilder.AddForeignKey(
                name: "FK_IntersectionOfSubgroups_Classes_Class",
                table: "IntersectionOfSubgroups",
                column: "Class",
                principalTable: "Classes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_IntersectionOfSubgroups_Classes_Class",
                table: "IntersectionOfSubgroups");

            migrationBuilder.DropIndex(
                name: "IX_IntersectionOfSubgroups_Class",
                table: "IntersectionOfSubgroups");

            migrationBuilder.DropColumn(
                name: "Class",
                table: "IntersectionOfSubgroups");
        }
    }
}

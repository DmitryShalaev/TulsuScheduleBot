using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class remove_Class : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomDiscipline_Classes_Class",
                table: "CustomDiscipline");

            migrationBuilder.DropIndex(
                name: "IX_CustomDiscipline_Class",
                table: "CustomDiscipline");

            migrationBuilder.DropColumn(
                name: "Class",
                table: "CustomDiscipline");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<byte>(
                name: "Class",
                table: "CustomDiscipline",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_CustomDiscipline_Class",
                table: "CustomDiscipline",
                column: "Class");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomDiscipline_Classes_Class",
                table: "CustomDiscipline",
                column: "Class",
                principalTable: "Classes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ScheduleBot.Migrations {
    /// <inheritdoc />
    public partial class renameTypeDTO_to_ClassDTO : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedDisciplines_Types_Class",
                table: "CompletedDisciplines");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomDiscipline_Types_Class",
                table: "CustomDiscipline");

            migrationBuilder.DropForeignKey(
                name: "FK_Disciplines_Types_Class",
                table: "Disciplines");

            migrationBuilder.DropTable(
                name: "Types");

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new {
                    ID = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Classes", x => x.ID);
                });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { (byte)0, "all" },
                    { (byte)1, "lab" },
                    { (byte)2, "practice" },
                    { (byte)3, "lecture" },
                    { (byte)4, "other" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedDisciplines_Classes_Class",
                table: "CompletedDisciplines",
                column: "Class",
                principalTable: "Classes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomDiscipline_Classes_Class",
                table: "CustomDiscipline",
                column: "Class",
                principalTable: "Classes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Disciplines_Classes_Class",
                table: "Disciplines",
                column: "Class",
                principalTable: "Classes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedDisciplines_Classes_Class",
                table: "CompletedDisciplines");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomDiscipline_Classes_Class",
                table: "CustomDiscipline");

            migrationBuilder.DropForeignKey(
                name: "FK_Disciplines_Classes_Class",
                table: "Disciplines");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.CreateTable(
                name: "Types",
                columns: table => new {
                    ID = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Types", x => x.ID);
                });

            migrationBuilder.InsertData(
                table: "Types",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { (byte)0, "all" },
                    { (byte)1, "lab" },
                    { (byte)2, "practice" },
                    { (byte)3, "lecture" },
                    { (byte)4, "other" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedDisciplines_Types_Class",
                table: "CompletedDisciplines",
                column: "Class",
                principalTable: "Types",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomDiscipline_Types_Class",
                table: "CustomDiscipline",
                column: "Class",
                principalTable: "Types",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Disciplines_Types_Class",
                table: "Disciplines",
                column: "Class",
                principalTable: "Types",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
